import {
  Border,
  Borders,
  Buffer,
  CalculationProperties,
  CellValue,
  Column,
  Style,
  Workbook,
  WorkbookProperties,
  WorkbookView,
  Worksheet,
} from "exceljs";

export type Position = {
  c: number;
  r: number;
};
export type ExcelBorders = Partial<Borders> & {
  outside?: Border;
  [key: string]: Border | undefined;
};

export type ExcelStyle = Partial<Style> & {
  border?: ExcelBorders;
  height?: number;
};

export type ExcelCell = {
  start: Position;
  end?: Position;
  value?: CellValue;
  numberFormat?: string;
  style?: ExcelStyle;
  isNumFmt?: boolean;
  isCurrency?: boolean;
  customFormat?: string;
};

export type ExcelColumnDictionary = Record<string | number, Partial<Column>>;
export type ExcelColumns = Partial<Column>[];

export function convertObjectsToRows<T = Record<string, any>>(
  input: T[],
  keys?: Array<
    string | ((item: T, rowIndex?: number, columnIndex?: number) => unknown)
  >,
): unknown[][] {
  if (!input?.length) {
    return [];
  }
  keys = keys || Object.keys(input[0] as object);

  return input.map((m, y) =>
    keys.map((k, x) =>
      typeof k === "function" ? k(m, y, x) : (m as Record<string, unknown>)[k],
    ),
  );
}

const buildBorderStyle = (borders: ExcelBorders): Partial<Borders> => {
  if (!borders) {
    return {};
  }

  let borderTypes = Object.keys(borders);
  if (!borderTypes.length) {
    return {};
  }

  const hasOutsideBorder = borderTypes.includes("outside");
  if (!hasOutsideBorder) {
    return borders;
  }

  borderTypes = borderTypes.filter(
    (k) => !["top", "left", "bottom", "right", "outside"].includes(k),
  );

  return {
    ...borderTypes.reduce(
      (acc, k) => ({
        ...acc,
        [k]: borders[k],
      }),
      {},
    ),
    top: borders.outside,
    left: borders.outside,
    right: borders.outside,
    bottom: borders.outside,
  };
};

export type ExcelBuilderOptions = {
  category?: string;
  company?: string;
  creator?: string;
  description?: string;
  keywords?: string;
  lastModifiedBy?: string;
  created?: Date;
  manager?: string;
  modified?: Date;
  lastPrinted?: Date;
  properties?: WorkbookProperties;
  subject?: string;
  title?: string;
  /**
   * Workbook calculation Properties
   */
  calcProperties?: CalculationProperties;
  /**
   * The Workbook views controls how many separate windows Excel will open when viewing the workbook.
   */
  views?: WorkbookView[];

  sheetName?: string;
};

export const createExcelBuilder = (options?: ExcelBuilderOptions) => {
  const workbook = new Workbook();
  workbook.created = new Date();
  workbook.modified = new Date();
  workbook.lastPrinted = new Date();

  if (options) {
    Object.keys(options).forEach((k) => {
      if (k === "sheetName") {
        return;
      }

      if (
        (workbook as object).hasOwnProperty(k) &&
        typeof (workbook as any)[k] !== "function"
      ) {
        (workbook as any)[k] = (options as any)[k];
      }
    });
  }
  const worksheet = workbook.addWorksheet(options?.sheetName ?? "Sheet 1");
  const write = (): Promise<Buffer> => {
    return workbook.xlsx.writeBuffer();
  };

  const useSheet = (name: string) => {
    if (worksheet.name === name) {
      return getSelf();
    }
    const sh = workbook.getWorksheet(name);

    return {
      ...getSelf(),
      worksheet: sh,
    };
  };

  const setColumn = (indexOrKey: number | string, value: Partial<Column>) => {
    const col = worksheet.getColumn(indexOrKey);
    Object.keys(value).forEach((k) => {
      if (
        (col as object).hasOwnProperty(k) &&
        typeof (col as any)[k] !== "function"
      ) {
        (col as any)[k] = (value as any)[k];
      }
    });

    return getSelf();
  };

  const setColumns = (columns: ExcelColumns | ExcelColumnDictionary) => {
    if (Array.isArray(columns)) {
      worksheet.columns = columns;
    } else {
      Object.keys(columns).forEach((k) => {
        try {
          const index = parseInt(k, 10);
          setColumn(index, columns[k]);
        } catch {
          setColumn(k, columns[k]);
        }
      });
    }

    return getSelf();
  };

  const addCells = (cells: ExcelCell[], style?: ExcelStyle) => {
    cells.forEach((c) => {
      const rowPositions = [c.start.r, c.end?.r]
        .filter((v) => typeof v === "number")
        .sort((a, b) => a - b);
      const colPositions = [c.start.c, c.end?.c]
        .filter((v) => typeof v === "number")
        .sort((a, b) => a - b);

      const cell = worksheet.getCell(rowPositions[0], colPositions[0]);

      if (c.isNumFmt) {
        worksheet.getColumn(c.start.c).numFmt = "#,##0.##0";
      }

      if (c.isCurrency) {
        worksheet.getColumn(c.start.c).numFmt = "#,##0";
      }

      if (c.customFormat) {
        worksheet.getColumn(c.start.c).numFmt = c.customFormat;
      }

      if (c.value) {
        cell.value = c.value;
      }

      if (style || c.style) {
        cell.style = {
          ...style,
          ...c.style,
          border: buildBorderStyle(style?.border ?? c.style?.border ?? {}),
        };
      }
      if (c.numberFormat && typeof c.value === "number") {
        cell.numFmt = c.numberFormat;
      }

      if (c.end) {
        worksheet.mergeCells(
          rowPositions[0],
          colPositions[0],
          rowPositions[1],
          colPositions[1],
        );
      }

      if (c.end && c.start.c !== c.end.c) {
        return;
      }

      // set auto width
      const col = worksheet.getColumn(c.start.c);
      col.width = Math.max(estimateCellWidth(c), col.width || 0);
    });

    return getSelf();
  };

  const addRows = <T>(rows: T[][], style?: ExcelStyle, isNumFmt?: boolean) => {
    worksheet.addRows(rows, style as string);
    if (isNumFmt) {
      worksheet.getCell("C29").numFmt = "#,##0.000";
    }

    return getSelf();
  };

  const addRowsCustom = <T>(
    rows: T[][],
    style?: ExcelStyle,
    isFirst?: boolean,
  ) => {
    rows.forEach((row) => {
      const newRow = worksheet.addRow(row);
      if (style) {
        row.forEach((cell, index) => {
          if (isFirst) {
            newRow.getCell(index + 1).style = style;
          } else {
            if (index !== 0) {
              newRow.getCell(index + 1).style = style;
            }
          }
        });
      }
    });
    return getSelf();
  };

  const rowsCustomStyle = (indexRows: number[], style: ExcelStyle) => {
    indexRows.forEach((r) => {
      if (style.font) {
        worksheet.getRow(r).font = style.font;
      }
      if (style.height) {
        worksheet.getRow(r).height = style.height;
      }
    });
  };

  const columnsCustomStyle = (
    indexColumns: {
      column: number;
      style?: ExcelStyle;
      width?: number;
    }[],
    styles?: ExcelStyle,
    width?: number,
  ) => {
    indexColumns.forEach((c) => {
      if (c.style || styles) {
        worksheet.getColumn(c.column).style = {
          ...styles,
          ...c.style,
        };
      }
      if (c.width || width) {
        worksheet.getColumn(c.column).width = c.width ?? width;
      }
    });
  };

  const cellsCustomStyle = (indexColumns: string[], style: ExcelStyle) => {
    indexColumns.forEach((c) => {
      worksheet.getCell(c).style = style;
    });
  };

  const rowsCustomCurrent = (indexRows: number[]) => {
    indexRows.forEach((r) => {
      // for (let rowIndex = 1; rowIndex <= worksheet.rowCount; rowIndex++) {
      //   let cellValueNumber: number;
      //   const cell = worksheet.getCell(r, rowIndex);
      //   const cellValue = cell.value;

      //   if (typeof cellValue === "string" && cellValue.includes(",")) {
      //     cellValueNumber = convertCurrencyToNumber(cellValue);
      //     if (cellValue) {
      //       cell.value = cellValueNumber;
      //       cell.numFmt = "#,##0";
      //     }
      //   } else {
      //     if (cellValue) {
      //       cell.value = Number(cellValue);
      //     }
      //   }
      // }
      worksheet.getRow(r).numFmt = "#,##0";
    });
  };
  const cellsCustomCurrent = (
    indexCells: string[],
    numberFormat = "#,##0.#0",
  ) => {
    indexCells.forEach((r) => {
      worksheet.getCell(r).numFmt = numberFormat;
    });
  };
  const mergeCells = (indexCells: string[]) => {
    /**
     * Merge cells, either:
     *
     * e.g. `'A4:B5'`
     *
     * e.g. `'G10', 'H11'`
     *
     * e.g. `10,11,12,13`
     */
    indexCells.forEach((cell) => {
      worksheet.mergeCells(cell);
    });
  };

  const addSheet = (
    name: string,
    rows?: any,
    cells?: ExcelCell[],
    styleExcel?: ExcelStyle,
    rowsCustom?: any,
    columns?: ExcelColumns | ExcelColumnDictionary,
  ) => {
    const worksheet = workbook.addWorksheet(name);
    const addCells = (cells: ExcelCell[], style?: ExcelStyle) => {
      cells.forEach((c) => {
        const rowPositions = [c.start.r, c.end?.r]
          .filter((v) => typeof v === "number")
          .sort((a, b) => a - b);
        const colPositions = [c.start.c, c.end?.c]
          .filter((v) => typeof v === "number")
          .sort((a, b) => a - b);

        const cell = worksheet.getCell(rowPositions[0], colPositions[0]);

        if (c.isNumFmt) {
          worksheet.getColumn(c.start.c).numFmt = "0.00";
        }

        if (c.value) {
          cell.value = c.value;
        }

        if (style || c.style) {
          cell.style = {
            ...style,
            ...c.style,
            border: buildBorderStyle(style?.border ?? c.style?.border ?? {}),
          };
        }
        if (c.numberFormat && typeof c.value === "number") {
          cell.numFmt = c.numberFormat;
        }

        if (c.end) {
          worksheet.mergeCells(
            rowPositions[0],
            colPositions[0],
            rowPositions[1],
            colPositions[1],
          );
        }

        if (c.end && c.start.c !== c.end.c) {
          return;
        }

        // set auto width
        const col = worksheet.getColumn(c.start.c);
        col.width = Math.max(estimateCellWidth(c), col.width || 0);
      });

      return getSelf();
    };

    if (rowsCustom) {
      worksheet.addRows(rowsCustom);
    }
    if (cells) {
      addCells(cells, styleExcel);
    }
    if (rows) {
      worksheet.addRows(rows);
    }
    if (columns) {
      if (Array.isArray(columns)) {
        worksheet.columns = columns;
      } else {
        Object.keys(columns).forEach((k) => {
          try {
            const index = parseInt(k, 10);
            setColumn(index, columns[k]);
          } catch {
            setColumn(k, columns[k]);
          }
        });
      }
    }
    return {
      ...getSelf(),
      worksheet: worksheet,
    };
  };
  const autoAdjustColumnWidth = (worksheet: Worksheet, startRow = 1) => {
    worksheet.columns.forEach((column) => {
      let maxLength = 0;
      if (typeof column.eachCell === "function") {
        column.eachCell({ includeEmpty: true }, (cell, row) => {
          if (row >= startRow) {
            const cellValue = cell.value;
            let cellLength = 0;

            if (cellValue) {
              switch (typeof cellValue) {
                case "string":
                  cellLength = cellValue.length;
                  break;
                case "number":
                  cellLength =
                    Math.round(cellValue).toString().length +
                    Math.ceil(cellValue.toString().length / 3);
                  // cellValue.toString().length +
                  // Math.ceil(cellValue.toString().length / 3);
                  break;
                case "boolean":
                  cellLength = 7;
                  break;
                default:
                  cellLength =
                    (cellValue.toString && cellValue.toString().length) || 0;
              }
            }

            if (cellLength > maxLength) {
              maxLength = cellLength;
            }
          }
        });
      }
      if (maxLength > 500) {
        column.width = 500;
      } else {
        column.width = maxLength > 90 ? maxLength - 10 : maxLength + 2;
      }
    });
  };

  const getSelf = () => ({
    workbook,
    worksheet,
    write,
    useSheet,
    addRows,
    addCells,
    setColumns,
    setColumn,
    addSheet,
    addRowsCustom,
    rowsCustomStyle,
    columnsCustomStyle,
    cellsCustomStyle,
    autoAdjustColumnWidth,
    rowsCustomCurrent,
    cellsCustomCurrent,
    mergeCells,
  });

  return getSelf();
};

const estimateCellWidth = (c: ExcelCell): number => {
  let width = 4;
  if (c.value) {
    switch (typeof c.value) {
      case "string":
        width = c.value.length;
        break;
      case "number":
        width = c.numberFormat
          ? Math.max(c.value.toString(10).length, c.numberFormat.length)
          : c.value.toString(10).length;
        break;
      case "boolean":
        width = 7;
        break;
      case "object":
        width = c.value.toString().length;
        break;
    }
  }

  return width * 1.5;
};

export const excelDateNumberToJSDate = (serial: number) => {
  const utc_days = Math.floor(serial - 25569);
  const utc_value = utc_days * 86400;
  const date_info = new Date(utc_value * 1000);

  const fractional_day = serial - Math.floor(serial) + 0.0000001;

  let total_seconds = Math.floor(86400 * fractional_day);

  const seconds = total_seconds % 60;

  total_seconds -= seconds;

  const hours = Math.floor(total_seconds / (60 * 60));
  const minutes = Math.floor(total_seconds / 60) % 60;

  return new Date(
    date_info.getFullYear(),
    date_info.getMonth(),
    date_info.getDate(),
    hours,
    minutes,
    seconds,
  );
};

interface ExportFileOpts {
  mimeType?: string;
  byteOrderMark?: string | Uint8Array;
  encoding?: string;
}

export function exportFile(
  fileName: string,
  rawData: string | ArrayBuffer | ArrayBufferView | Blob,
  opts: string | ExportFileOpts = { mimeType: "application/octet-stream" },
): true | Error {
  if (!fileName || !rawData) {
    return new Error("Missing fileName or rawData");
  }

  try {
    // Xử lý opts nếu là string
    const options: ExportFileOpts =
      typeof opts === "string" ? { mimeType: opts } : opts;

    // Tạo Blob từ rawData
    const blob =
      rawData instanceof Blob
        ? rawData
        : new Blob(
            [
              ArrayBuffer.isView(rawData)
                ? new Uint8Array(
                    new Uint8Array(
                      rawData.buffer,
                      rawData.byteOffset,
                      rawData.byteLength,
                    ),
                  )
                : rawData,
            ],
            { type: options.mimeType },
          );

    // Tạo URL tạm thời cho Blob
    const url = window.URL.createObjectURL(blob);

    // Tạo thẻ <a> để kích hoạt tải xuống
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();

    // Dọn dẹp
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);

    return true;
  } catch (error) {
    return new Error(`Failed to export file: ${error}`);
  }
}
