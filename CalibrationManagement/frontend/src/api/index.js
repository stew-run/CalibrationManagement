// Barrel: import every endpoint group from '../api' (or '../api/index').
export { default as api, API_BASE_URL, ApiError, clean } from './axiosClient';
export * from './authApi';
export * from './healthApi';
export * from './lookupApi';
export * from './dashboardApi';
export * from './equipmentApi';
export * from './scheduleApi';
export * from './resultApi';
export * from './historyApi';
export * from './certificateApi';
export * from './maintenanceApi';
export * from './notificationApi';
export * from './reportApi';
export * from './userApi';
