export interface User {
  id: string;
  displayName: string;
  email: string;
  token: string;
  firstName?: string;
  lastName?: string;
  imageUrl?: string;
}

export interface LoginCreds {
  email: string;
  password: string;
}

export interface RegisterCreds {
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  password: string;
  gender: string;
  dateOfBirth: string;
  city: string;
  country: string;
}
