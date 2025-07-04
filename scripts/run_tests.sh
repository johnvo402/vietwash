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
