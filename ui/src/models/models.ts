export interface ITestResult {
  number: number;
  name: string;
  passed: boolean;
}
export interface IStatusMessage {
  status: string;
  testResult?: ITestResult;
  success: boolean;
}
