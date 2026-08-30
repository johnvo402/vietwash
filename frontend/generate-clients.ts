const fs = require("fs");
const path = require("path");

async function mergeOpenApiFiles(folderPath: string, outputPath: string) {
  const merged = {
    openapi: "3.0.0",
    info: {
      title: "Merged API",
      version: "1.0.0",
    },
    servers: [
      {
        url: "https://server.ttexe.id.vn",
        description: "Local server",
      },
    ],
    paths: {},
    components: {
      schemas: {},
    },
  };

  // Đọc tất cả các file trong thư mục
  const files = fs.readdirSync(folderPath);

  // Duyệt qua các file JSON và gộp chúng lại
  for (const file of files) {
    const filePath = path.join(folderPath, file);
    if (file.endsWith(".json")) {
      console.log(`Processing file: ${file}`);
      const content = JSON.parse(fs.readFileSync(filePath, "utf-8"));

      // Duyệt qua các paths và bỏ phần "tags" nếu có
      for (const pathKey in content.paths) {
        if (content.paths[pathKey].post)
          content.paths[pathKey].post.tags = ["Client"];
        if (content.paths[pathKey].get)
          content.paths[pathKey].get.tags = ["Client"];
        if (content.paths[pathKey].put)
          content.paths[pathKey].put.tags = ["Client"];
        if (content.paths[pathKey].delete)
          content.paths[pathKey].delete.tags = ["Client"];
      }

      // Gộp dữ liệu vào tài liệu Swagger hợp nhất
      Object.assign(merged.paths, content.paths);
      Object.assign(merged.components.schemas, content.components?.schemas);
    }
  }

  // Lưu tài liệu Swagger hợp nhất ra file
  fs.writeFileSync(outputPath, JSON.stringify(merged, null, 2));
  console.log(`Merged OpenAPI saved to ${outputPath}`);
}

// Đường dẫn đến thư mục chứa các file JSON và file output
const inputFolderPath = "./src/openapi"; // Thư mục chứa các file JSON
const outputFilePath = "./openapi.json"; // Tên file output sau khi gộp

// Gọi hàm để gộp các file JSON
mergeOpenApiFiles(inputFolderPath, outputFilePath).catch(console.error);
