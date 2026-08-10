export interface UserSummaryDto {
  id: number;
  username: string;
  role: string;
  olusturmaTarihi: string;
}

export interface CreateUserRequestDto {
  username: string;
  password: string;
  role: string;
}
