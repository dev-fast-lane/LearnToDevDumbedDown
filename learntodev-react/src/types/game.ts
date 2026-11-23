// Samme felt som PlayerStatusDto i API-et. API-et sender camelCase
export interface PlayerStatusDto {
    currentJobName: string | null;
    passedSchools: string[];
}