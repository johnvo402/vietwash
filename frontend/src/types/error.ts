interface ErrorDetail {
  message: string;
  en: string;
  vi: string;
}
interface InvalidParam {
  propertyName: string;
  reasons?: ErrorDetail[] | null;
}
export interface RequestError {
  type: string;
  title: string;
  status: number;
  ErrorDetail?: ErrorDetail | any;
  invalidParams?: InvalidParam[] | null;
}
