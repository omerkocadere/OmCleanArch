export interface ValidationError {
  code: string;
  description: string;
  type: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: ValidationError[]; // Validation errors array
  [key: string]: unknown; // Allow additional properties
}
