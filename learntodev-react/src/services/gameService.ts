import { get } from "./apiClient";
import type { PlayerStatusDto } from "../types/game";

// Alle kall til /api/game. Komponentene bruker denne, ikke fetch
export function getStatus(): Promise<PlayerStatusDto> {
    return get<PlayerStatusDto>("/game/status");
}