using System.ComponentModel.DataAnnotations;

namespace UrbanCommunity.Domain.Enums;

public enum ReportStatus
{
    [Display(Name = "Onay Bekliyor")]
    PendingReview = 1,
    [Display(Name = "Yapay Zeka İncelemesinde")]
    AiReview = 2,
    [Display(Name = "Onaylandı")]
    Confirmed = 3,
    [Display(Name = "İşleme Alındı")]
    InProgress = 4,
    [Display(Name = "Çözüldü")]
    Resolved = 5,
    [Display(Name = "Reddedildi")]
    Rejected = 6
}

public enum ReportPriority
{
    [Display(Name = "Düşük")]
    Low = 1,
    [Display(Name = "Orta")]
    Medium = 2,
    [Display(Name = "Yüksek")]
    High = 3,
    [Display(Name = "Acil")]
    Urgent = 4
}
