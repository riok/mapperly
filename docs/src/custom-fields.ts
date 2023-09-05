export interface CustomFields {
  mapperlyVersion: string;
  environment: {
    name: string;
    stable: boolean;
    next: boolean;
    local: boolean;
  };
}
