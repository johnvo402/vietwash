#!/bin/bash

# Chuyển đến thư mục gốc của dự án
ROOT="$(dirname "${BASH_SOURCE[0]}")/.."
cd "$ROOT" || { echo "Lỗi: Không thể chuyển đến thư mục gốc $ROOT"; exit 1; }


# Kiểm tra xem biến NAME có được truyền vào không
if [ -z "$NAME" ]; then
  echo "Lỗi: Vui lòng cung cấp biến NAME với danh sách tên hàm test."
  echo "Ví dụ: NAME=\"AuthService.Tests.Accounts.CreateAccountCommandValidatorTests.Validate_WhenEmailNullOrEmpty_ShouldReturnNullFailure AuthService.Tests.Accounts.CreateAccountCommandValidatorTests.Validate_WhenEmailInvalidFormat_ShouldReturnInvalidFailure\" ./run_tests_with_coverlet.sh"
  exit 1
fi
TEST_PROJECT_DIR="$ROOT/tests/UnitTest/AuthSerivce.Tests"
if [ ! -d "$TEST_PROJECT_DIR" ]; then
  echo "Lỗi: Thư mục dự án test '$TEST_PROJECT_DIR' không tồn tại."
  exit 1
fi
cd "$TEST_PROJECT_DIR" || { echo "Lỗi: Không thể chuyển đến thư mục dự án test $TEST_PROJECT_DIR"; exit 1; }
# Chuyển đổi chuỗi NAME thành mảng các tên hàm test
IFS=' ' read -r -a TEST_NAMES <<< "$NAME"

# Tạo bộ lọc cho dotnet test
FILTER=""
for i in "${!TEST_NAMES[@]}"; do
  if [ $i -eq 0 ]; then
    FILTER="FullyQualifiedName~${TEST_NAMES[$i]}"
  else
    FILTER="$FILTER|FullyQualifiedName~${TEST_NAMES[$i]}"
  fi
done

# Thư mục đầu ra cho báo cáo kết quả test
OUTPUT_DIR="../TestResults"
mkdir -p "$OUTPUT_DIR"

# Chạy lệnh dotnet test với logger JUnit
echo "Đang chạy các test: $NAME và tạo báo cáo kết quả..."
dotnet test --filter "$FILTER" \
  --collect:"XPlat Code Coverage;Format=cobertura" \
  --logger "junit;LogFilePath=$OUTPUT_DIR/test-results.xml" \
  --results-directory "$OUTPUT_DIR" \
  --logger "console;verbosity=normal"

# Kiểm tra kết quả của lệnh dotnet test
if [ $? -eq 0 ]; then
  echo "Chạy test thành công!"
else
  echo "Chạy test thất bại. Vui lòng kiểm tra lại bộ lọc hoặc dự án test."
  echo "Bạn có thể chạy lệnh sau để debug: dotnet test --filter \"$FILTER\" --logger \"junit;LogFilePath=$OUTPUT_DIR/test-results.xml\" --logger \"console;verbosity=detailed\""
  exit 1
fi

if command -v reportgenerator >/dev/null 2>&1; then
  echo "Tạo báo cáo coverage dạng HTML và hợp nhất file coverage..."
  reportgenerator -reports:"$OUTPUT_DIR/**/*.cobertura.xml" \
    -targetdir:"$OUTPUT_DIR/html" \
    -reporttypes:"HtmlInline_AzurePipelines_Dark;Cobertura" \
    -filefilters:"-*\obj\*" \
    -verbosity:Error
  if [ $? -eq 0 ]; then
    echo "Báo cáo HTML được lưu tại: $OUTPUT_DIR/html"
    # Di chuyển file Cobertura.xml sang UnitTestResults/coverage.cobertura.xml
    if [ -f "$OUTPUT_DIR/html/Cobertura.xml" ]; then
      mv "$OUTPUT_DIR/html/Cobertura.xml" "$OUTPUT_DIR/coverage.cobertura.xml"
      if [ $? -eq 0 ]; then
        echo "File coverage hợp nhất được lưu tại: $OUTPUT_DIR/coverage.cobertura.xml"
      else
        echo "Lỗi: Không thể di chuyển file Cobertura.xml sang $OUTPUT_DIR/coverage.cobertura.xml."
        exit 1
      fi
    else
      echo "Lỗi: File $OUTPUT_DIR/html/Cobertura.xml không tồn tại."
      exit 1
    fi
  else
    echo "Lỗi: Không thể tạo báo cáo HTML hoặc hợp nhất file coverage. Kiểm tra cài đặt reportgenerator và file coverage."
    exit 1
  fi
else
  echo "Lỗi: reportgenerator không được cài đặt. Vui lòng cài đặt để tạo báo cáo HTML và hợp nhất file coverage."
  echo "Cài đặt bằng: dotnet tool install -g dotnet-reportgenerator-globaltool"
  exit 1
fi
