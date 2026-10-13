/**
 * API contracts shared across features (JSDoc only - the project is plain JavaScript).
 * Property names match the camelCase JSON the ASP.NET Core API returns.
 *
 * @typedef {'Administrator'|'QualityManager'|'Technician'|'Viewer'} RoleName
 * @typedef {'read'|'write'|'manage'|'admin'} Permission
 *
 * @typedef {Object} User
 * @property {number} userId
 * @property {string} username
 * @property {string} email
 * @property {string} fullName
 * @property {number} roleId
 * @property {RoleName} role
 * @property {boolean} isActive
 * @property {string|null} lastLoginAt
 * @property {string} createdAt
 *
 * @typedef {Object} LoginResponse
 * @property {string} token
 * @property {string} expiresAtUtc
 * @property {User} user
 *
 * @typedef {Object} PagedResult
 * @property {Array} items
 * @property {number} page
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 */
export {};
