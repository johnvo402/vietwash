#!/bin/bash

# Gán giá trị từ tham số dòng lệnh nếu có
if [ -n "$3" ]; then
  NAME="$3"
fi
if [ -n "$2" ]; then
  SERVICE="$2"
fi
if [ -n "$1" ]; then
  TYPE="$1"
fi
# Chuyển đến thư mục gốc của dự án
ROOT="$(pwd)"
cd "$ROOT" || {
  echo "Lỗi: Không thể chuyển đến thư mục gốc $ROOT"
  exit 1
}

# Thư mục kết quả chung
OUTPUT_DIR="TestResults"

if [[ ! "$TYPE" =~ ^[a-zA-Z0-9_,_-]*$ ]]; then
  echo "Lỗi: Biến TYPE chứa ký tự không hợp lệ."
  exit 1
fi

IFS=',' read -r -a TEST_TYPE <<<"$TYPE"

# Nếu truyền SERVICE thì chạy test theo từng service
if [ -n "$SERVICE" ]; then
  if [[ ! "$SERVICE" =~ ^[a-zA-Z0-9_,_-]*$ ]]; then
    echo "Lỗi: Biến SERVICE chứa ký tự không hợp lệ."
    exit 1
  fi

  declare -a COVERAGE_FILES

  TEST_PROJECT_DIR="$ROOT/tests/${TEST_TYPE}/${SERVICE}.Tests"

  # Kiểm tra lỗi chính tả trong tên thư mục
  if [ ! -d "$TEST_PROJECT_DIR" ]; then
    echo "Lỗi: Thư mục dự án test '$TEST_PROJECT_DIR' không tồn tại."
    echo "Kiểm tra xem tên thư mục có đúng không (ví dụ: AuthService.Tests, không phải AuthSerivce.Tests)."
    exit 1
  fi

  cd "$TEST_PROJECT_DIR" || {
    echo "Lỗi: Không thể chuyển đến thư mục dự án test $TEST_PROJECT_DIR"
    exit 1
  }

  if [ -n "$NAME" ]; then
    if [[ ! "$NAME" =~ ^[a-zA-Z0-9_,_-]*$ ]]; then
      echo "Lỗi: Biến NAME chứa ký tự không hợp lệ."
      exit 1
    fi

    IFS=',' read -r -a TEST_NAMES <<<"$NAME"
    FILTER=""
    for i in "${!TEST_NAMES[@]}"; do
      if [ $i -eq 0 ]; then
        FILTER="FullyQualifiedName~${TEST_NAMES[$i]}"
      else
        FILTER="$FILTER|FullyQualifiedName~${TEST_NAMES[$i]}"
      fi
    done

    echo "Đang chạy test cho service: $SERVICE các test: $NAME..."
    TEST_OUTPUT=$(dotnet test --filter "$FILTER" \
      --collect:"XPlat Code Coverage;Format=cobertura" \
      --logger "junit;LogFilePath=$OUTPUT_DIR/test-results-$SERVICE.xml" \
      --results-directory "$OUTPUT_DIR" \
      --logger "console;verbosity=detailed" 2>&1)
    if echo "$TEST_OUTPUT" | grep -q "No test is available"; then
      echo "Lỗi: Không tìm thấy test nào khớp với bộ lọc '$FILTER' cho service '$SERVICE'."
      exit 1
    fi
  else
    echo "Đang chạy test cho service: $SERVICE..."
    dotnet test "$TEST_PROJECT_DIR" \
      --collect:"XPlat Code Coverage;Format=cobertura" \
      --logger "junit;LogFilePath=$OUTPUT_DIR/test-results-$SERVICE.xml" \
      --results-directory "$OUTPUT_DIR" \
      --logger "console;verbosity=normal"
  fi

  if [ $? -ne 0 ]; then
    echo "Chạy test thất bại cho service: $SERVICE"
  fi

  # Thu thập file coverage từ thư mục TestResults của dự án test
  TEST_PROJECT_RESULTS="$TEST_PROJECT_DIR/TestResults"
  if [ -d "$TEST_PROJECT_RESULTS" ]; then
    # Tìm file .cobertura.xml trong thư mục con có GUID
    NEW_FILES=("$TEST_PROJECT_RESULTS"/*/*.cobertura.xml)
    if [ -e "${NEW_FILES[0]}" ]; then
      for file in "${NEW_FILES[@]}"; do
        # Di chuyển file coverage sang $OUTPUT_DIR với tên duy nhất
        mv "$file" "$OUTPUT_DIR/coverage-$SERVICE-$(basename "$file")" || {
          echo "Lỗi: Không thể di chuyển file $file sang $OUTPUT_DIR"
          exit 1
        }
      done
      COVERAGE_FILES+=("$OUTPUT_DIR/coverage-$SERVICE-"*.cobertura.xml)
    else
      echo "Lỗi: Không tìm thấy file .cobertura.xml trong $TEST_PROJECT_RESULTS"
      exit 1
    fi
  else
    echo "Lỗi: Thư mục $TEST_PROJECT_RESULTS không tồn tại"
    exit 1
  fi

  # Tạo báo cáo coverage
  if [ ${#COVERAGE_FILES[@]} -eq 0 ]; then
    echo "Lỗi: Không tìm thấy file coverage nào."
    exit 1
  fi

  if dotnet tool list --global | grep -q "dotnet-reportgenerator-globaltool" || dotnet tool list --local | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "Tạo báo cáo coverage..."
    REPORT_FILES_STR=$(
      IFS=';'
      echo "${COVERAGE_FILES[*]}"
    )
    dotnet tool run reportgenerator -reports:"$REPORT_FILES_STR" \
      -targetdir:"$OUTPUT_DIR/result" \
      -reporttypes:"Html;TextSummary" \
      -filefilters:"-*\obj\*" \
      -sourcedirs:"$ROOT" \
      -verbosity:Error
    if [ $? -ne 0 ]; then
      echo "Lỗi: Tạo báo cáo coverage thất bại."
      exit 1
    fi
    echo "Báo cáo được lưu tại: $OUTPUT_DIR/result"
  else
    echo "Lỗi: reportgenerator không được cài đặt."
    echo "Vui lòng cài bằng: dotnet new tool-manifest (nếu chưa có) và dotnet tool install dotnet-reportgenerator-globaltool"
    exit 1
  fi

# Nếu không truyền gì thì chạy full solution
else
  echo "Đang chạy toàn bộ solution..."
  dotnet test \
    --collect:"XPlat Code Coverage;Format=cobertura" \
    --logger "junit;LogFilePath=$OUTPUT_DIR/test-results-full.xml" \
    --results-directory "$OUTPUT_DIR" \
    --logger "console;verbosity=minimal"

  if [ $? -ne 0 ]; then
    echo "Chạy test toàn bộ solution thất bại."
  fi

  # Thu thập file coverage từ tất cả các thư mục TestResults trong tests/UnitTest
  REPORT_FILES=("$ROOT"/*/*/coverage.cobertura.xml)
  if [ ! -e "${REPORT_FILES[0]}" ]; then
    echo "Lỗi: Không tìm thấy file coverage nào trong $ROOT/tests/${TEST_TYPE}"
    exit 1
  fi

  # Di chuyển file coverage sang $OUTPUT_DIR
  for file in "${REPORT_FILES[@]}"; do
    mv "$file" "$OUTPUT_DIR/coverage-$(basename "$file")" || {
      echo "Lỗi: Không thể di chuyển file $file sang $OUTPUT_DIR"
      exit 1
    }
  done
  REPORT_FILES=("$OUTPUT_DIR/coverage-"*.cobertura.xml)

  if [ ! -e "${REPORT_FILES[0]}" ]; then
    echo "Lỗi: Không tìm thấy file coverage nào trong $OUTPUT_DIR."
    exit 1
  fi

  if dotnet tool list --global | grep -q "dotnet-reportgenerator-globaltool" || dotnet tool list --local | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "Tạo báo cáo coverage..."
    REPORT_FILES_STR=$(
      IFS=';'
      echo "${REPORT_FILES[*]}"
    )
    dotnet tool run reportgenerator -reports:"$REPORT_FILES_STR" \
      -targetdir:"$OUTPUT_DIR/result" \
      -reporttypes:"Html;TextSummary" \
      -filefilters:"-*\obj\*" \
      -sourcedirs:"$ROOT" \
      -verbosity:Error
    if [ $? -ne 0 ]; then
      echo "Lỗi: Tạo báo cáo coverage thất bại."
      exit 1
    fi
    echo "Báo cáo được lưu tại: $OUTPUT_DIR/result"
  else
    echo "Lỗi: reportgenerator không được cài đặt."
    echo "Vui lòng cài bằng: dotnet new tool-manifest (nếu chưa có) và dotnet tool install dotnet-reportgenerator-globaltool"
    exit 1
  fi
fi
