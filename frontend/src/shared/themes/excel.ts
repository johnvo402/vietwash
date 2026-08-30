import { ExcelStyle } from "../utils/excel";

export const excelHeaderStyle: ExcelStyle = {
  font: {
    name: "Times New Roman",
    family: 4,
    size: 12,
    underline: false,
    bold: true,
    italic: false,
    color: { argb: "FF080C1A" }, // foreground
  },
  alignment: {
    vertical: "middle",
    horizontal: "center",
  },
  border: {
    outside: {
      style: "thin",
      color: { argb: "FF080C1A" }, // foreground
    },
  },
  fill: {
    type: "pattern",
    pattern: "solid",
    fgColor: { argb: "FF3B82F6" }, // primary
    bgColor: { argb: "FF3B82F6" }, // primary
  },
};
