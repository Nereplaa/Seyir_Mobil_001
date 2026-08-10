export interface LoginRequestDto {
  username: string;
  password: string;
}

export interface LoginResponseDto {
  token: string;
  expiresAt: string;
  username: string;
  role: string;
}

export interface CurrentUserDto {
  username: string;
  role: string;
}
