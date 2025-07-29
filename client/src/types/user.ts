export type User = {
    id: string;
    displayName: string;
    email: string;
    token: string;
    firstName?: string;
    lastName?: string;
    imageUrl?: string;
}

export type LoginCreds = {
    email: string;
    password: string;
}

export type RegisterCreds = {
    email: string;
    displayName: string;
    firstName?: string;
    lastName?: string;
    password: string;
}