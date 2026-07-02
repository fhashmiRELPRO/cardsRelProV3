using RelPro.Contracts.Enums;

namespace RelPro.Contracts.Dtos;

public sealed class ContractEntitlementsDto
{
    public int ContractId { get; init; }

    public bool SearchPeople { get; init; }
    public bool SearchCompanies { get; init; }
    public bool SearchAdvanced { get; init; }
    public bool SearchBulk { get; init; }
    public bool SearchSavedSearches { get; init; }
    public bool SearchAlerts { get; init; }

    public bool ExportPeople { get; init; }
    public bool ExportCompanies { get; init; }
    public bool ExportBulk { get; init; }
    public bool ExportToCRM { get; init; }
    public bool ExportToExcel { get; init; }
    public bool ExportToCsv { get; init; }

    public bool CRMSalesforce { get; init; }
    public bool CRMHubSpot { get; init; }
    public bool CRMDynamics { get; init; }
    public bool CRMZoho { get; init; }
    public bool CRMPipedrive { get; init; }
    public bool CRMOutreach { get; init; }
    public bool CRMSalesloft { get; init; }

    public bool DataDirectDial { get; init; }
    public bool DataMobilePhone { get; init; }
    public bool DataPersonalEmail { get; init; }
    public bool DataWorkEmail { get; init; }
    public bool DataSocialProfiles { get; init; }
    public bool DataTechnographics { get; init; }
    public bool DataFirmographics { get; init; }
    public bool DataFunding { get; init; }
    public bool DataJobChanges { get; init; }
    public bool DataNewsletter { get; init; }

    public bool UserManagement { get; init; }
    public bool UserRoles { get; init; }
    public bool UserTeams { get; init; }
    public bool UserInvite { get; init; }
    public bool UserSSO { get; init; }
    public bool UserAuditLog { get; init; }

    public bool ListCreate { get; init; }
    public bool ListShare { get; init; }
    public bool ListImport { get; init; }
    public bool ListSuppression { get; init; }
    public bool ListEnrichment { get; init; }

    public bool VendorDataIntegration { get; init; }
    public bool VendorCustomData { get; init; }
    public bool VendorApiAccess { get; init; }
    public bool VendorWebhooks { get; init; }

    public bool ReportingBasic { get; init; }
    public bool ReportingAdvanced { get; init; }
    public bool ReportingDashboard { get; init; }
    public bool ReportingExport { get; init; }

    public bool AIRecommendations { get; init; }
    public bool AIInsights { get; init; }
    public bool AISmartSearch { get; init; }
    public bool AIChatAssist { get; init; }

    public bool AutomationSequences { get; init; }
    public bool AutomationWorkflows { get; init; }
    public bool AutomationScheduled { get; init; }
    public bool AutomationWebhookTrigger { get; init; }

    public bool FileUpload { get; init; }
    public bool FileStorage { get; init; }
    public bool FileSharing { get; init; }

    public bool EmailVerification { get; init; }
    public bool EmailTracking { get; init; }
    public bool EmailTemplates { get; init; }
    public bool EmailBulkSend { get; init; }

    public bool ApiAccess { get; init; }
    public bool ApiHighRateLimit { get; init; }
    public bool ApiWebhooks { get; init; }

    public bool PlatformMultiOrg { get; init; }
    public bool PlatformWhiteLabel { get; init; }
    public bool PlatformCustomBranding { get; init; }
    public bool PlatformSupportPriority { get; init; }
    public bool PlatformDedicatedCsm { get; init; }

    public bool ComplianceGdprTools { get; init; }
    public bool ComplianceCcpaTools { get; init; }
    public bool ComplianceDataRetention { get; init; }
    public bool ComplianceSoc2 { get; init; }

    public bool IsFeatureEnabled(EntitlementFeature feature) => feature switch
    {
        EntitlementFeature.SearchPeople => SearchPeople,
        EntitlementFeature.SearchCompanies => SearchCompanies,
        EntitlementFeature.SearchAdvanced => SearchAdvanced,
        EntitlementFeature.SearchBulk => SearchBulk,
        EntitlementFeature.SearchSavedSearches => SearchSavedSearches,
        EntitlementFeature.SearchAlerts => SearchAlerts,
        EntitlementFeature.ExportPeople => ExportPeople,
        EntitlementFeature.ExportCompanies => ExportCompanies,
        EntitlementFeature.ExportBulk => ExportBulk,
        EntitlementFeature.ExportToCRM => ExportToCRM,
        EntitlementFeature.ExportToExcel => ExportToExcel,
        EntitlementFeature.ExportToCsv => ExportToCsv,
        EntitlementFeature.CRMSalesforce => CRMSalesforce,
        EntitlementFeature.CRMHubSpot => CRMHubSpot,
        EntitlementFeature.CRMDynamics => CRMDynamics,
        EntitlementFeature.CRMZoho => CRMZoho,
        EntitlementFeature.CRMPipedrive => CRMPipedrive,
        EntitlementFeature.CRMOutreach => CRMOutreach,
        EntitlementFeature.CRMSalesloft => CRMSalesloft,
        EntitlementFeature.DataDirectDial => DataDirectDial,
        EntitlementFeature.DataMobilePhone => DataMobilePhone,
        EntitlementFeature.DataPersonalEmail => DataPersonalEmail,
        EntitlementFeature.DataWorkEmail => DataWorkEmail,
        EntitlementFeature.DataSocialProfiles => DataSocialProfiles,
        EntitlementFeature.DataTechnographics => DataTechnographics,
        EntitlementFeature.DataFirmographics => DataFirmographics,
        EntitlementFeature.DataFunding => DataFunding,
        EntitlementFeature.DataJobChanges => DataJobChanges,
        EntitlementFeature.DataNewsletter => DataNewsletter,
        EntitlementFeature.UserManagement => UserManagement,
        EntitlementFeature.UserRoles => UserRoles,
        EntitlementFeature.UserTeams => UserTeams,
        EntitlementFeature.UserInvite => UserInvite,
        EntitlementFeature.UserSSO => UserSSO,
        EntitlementFeature.UserAuditLog => UserAuditLog,
        EntitlementFeature.ListCreate => ListCreate,
        EntitlementFeature.ListShare => ListShare,
        EntitlementFeature.ListImport => ListImport,
        EntitlementFeature.ListSuppression => ListSuppression,
        EntitlementFeature.ListEnrichment => ListEnrichment,
        EntitlementFeature.VendorDataIntegration => VendorDataIntegration,
        EntitlementFeature.VendorCustomData => VendorCustomData,
        EntitlementFeature.VendorApiAccess => VendorApiAccess,
        EntitlementFeature.VendorWebhooks => VendorWebhooks,
        EntitlementFeature.ReportingBasic => ReportingBasic,
        EntitlementFeature.ReportingAdvanced => ReportingAdvanced,
        EntitlementFeature.ReportingDashboard => ReportingDashboard,
        EntitlementFeature.ReportingExport => ReportingExport,
        EntitlementFeature.AIRecommendations => AIRecommendations,
        EntitlementFeature.AIInsights => AIInsights,
        EntitlementFeature.AISmartSearch => AISmartSearch,
        EntitlementFeature.AIChatAssist => AIChatAssist,
        EntitlementFeature.AutomationSequences => AutomationSequences,
        EntitlementFeature.AutomationWorkflows => AutomationWorkflows,
        EntitlementFeature.AutomationScheduled => AutomationScheduled,
        EntitlementFeature.AutomationWebhookTrigger => AutomationWebhookTrigger,
        EntitlementFeature.FileUpload => FileUpload,
        EntitlementFeature.FileStorage => FileStorage,
        EntitlementFeature.FileSharing => FileSharing,
        EntitlementFeature.EmailVerification => EmailVerification,
        EntitlementFeature.EmailTracking => EmailTracking,
        EntitlementFeature.EmailTemplates => EmailTemplates,
        EntitlementFeature.EmailBulkSend => EmailBulkSend,
        EntitlementFeature.ApiAccess => ApiAccess,
        EntitlementFeature.ApiHighRateLimit => ApiHighRateLimit,
        EntitlementFeature.ApiWebhooks => ApiWebhooks,
        EntitlementFeature.PlatformMultiOrg => PlatformMultiOrg,
        EntitlementFeature.PlatformWhiteLabel => PlatformWhiteLabel,
        EntitlementFeature.PlatformCustomBranding => PlatformCustomBranding,
        EntitlementFeature.PlatformSupportPriority => PlatformSupportPriority,
        EntitlementFeature.PlatformDedicatedCsm => PlatformDedicatedCsm,
        EntitlementFeature.ComplianceGdprTools => ComplianceGdprTools,
        EntitlementFeature.ComplianceCcpaTools => ComplianceCcpaTools,
        EntitlementFeature.ComplianceDataRetention => ComplianceDataRetention,
        EntitlementFeature.ComplianceSoc2 => ComplianceSoc2,
        _ => false
    };
}
