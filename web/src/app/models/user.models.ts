export interface UserSummaryDto {
  id: number;
  username: string;
  role: string;
  email: string | null;
  olusturmaTarihi: string;
}

export interface CreateUserRequestDto {
  username: string;
  password: string;
  role: string;
  email: string;
}
