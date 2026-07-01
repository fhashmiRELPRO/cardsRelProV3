namespace RelPro.Contracts.Enums;

public enum EntitlementFeature
{
    // Search
    SearchPeople = 0,
    SearchCompanies = 1,
    SearchAdvanced = 2,
    SearchBulk = 3,
    SearchSavedSearches = 4,
    SearchAlerts = 5,

    // Export
    ExportPeople = 10,
    ExportCompanies = 11,
    ExportBulk = 12,
    ExportToCRM = 13,
    ExportToExcel = 14,
    ExportToCsv = 15,

    // CRM Integration
    CRMSalesforce = 20,
    CRMHubSpot = 21,
    CRMDynamics = 22,
    CRMZoho = 23,
    CRMPipedrive = 24,
    CRMOutreach = 25,
    CRMSalesloft = 26,

    // Data Access
    DataDirectDial = 30,
    DataMobilePhone = 31,
    DataPersonalEmail = 32,
    DataWorkEmail = 33,
    DataSocialProfiles = 34,
    DataTechnographics = 35,
    DataFirmographics = 36,
    DataFunding = 37,
    DataJobChanges = 38,
    DataNewsletter = 39,

    // User Management
    UserManagement = 40,
    UserRoles = 41,
    UserTeams = 42,
    UserInvite = 43,
    UserSSO = 44,
    UserAuditLog = 45,

    // List Management
    ListCreate = 50,
    ListShare = 51,
    ListImport = 52,
    ListSuppression = 53,
    ListEnrichment = 54,

    // Vendor Data
    VendorDataIntegration = 60,
    VendorCustomData = 61,
    VendorApiAccess = 62,
    VendorWebhooks = 63,

    // Reporting
    ReportingBasic = 70,
    ReportingAdvanced = 71,
    ReportingDashboard = 72,
    ReportingExport = 73,

    // AI Features
    AIRecommendations = 80,
    AIInsights = 81,
    AISmartSearch = 82,
    AIChatAssist = 83,

    // Automation
    AutomationSequences = 90,
    AutomationWorkflows = 91,
    AutomationScheduled = 92,
    AutomationWebhookTrigger = 93,

    // File Management
    FileUpload = 100,
    FileStorage = 101,
    FileSharing = 102,

    // Email
    EmailVerification = 110,
    EmailTracking = 111,
    EmailTemplates = 112,
    EmailBulkSend = 113,

    // API Access
    ApiAccess = 120,
    ApiHighRateLimit = 121,
    ApiWebhooks = 122,

    // Platform
    PlatformMultiOrg = 130,
    PlatformWhiteLabel = 131,
    PlatformCustomBranding = 132,
    PlatformSupportPriority = 133,
    PlatformDedicatedCsm = 134,

    // Compliance
    ComplianceGdprTools = 140,
    ComplianceCcpaTools = 141,
    ComplianceDataRetention = 142,
    ComplianceSoc2 = 143,

    // Legacy placeholder - kept for migration mapping
    LegacyReserved1 = 200,
    LegacyReserved2 = 201
}
