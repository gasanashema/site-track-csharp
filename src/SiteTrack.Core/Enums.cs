using System;

namespace SiteTrack.Core
{
    public enum UserRole
    {
        Administrator,
        SiteManager
    }

    public enum UserStatus
    {
        ACTIVE,
        INACTIVE
    }

    public enum ProjectStatus
    {
        Planning,
        Ongoing,
        Completed,
        Cancelled
    }

    public enum StockMovementType
    {
        IN,
        OUT,
        ADJUSTMENT
    }

    public enum AttendanceStatus
    {
        PRESENT,
        ABSENT,
        HALF_DAY
    }

    public enum PaymentStatus
    {
        PENDING,
        PAID
    }

    public enum NotificationChannel
    {
        EMAIL,
        SMS,
        SYSTEM
    }

    public enum NotificationStatus
    {
        PENDING,
        DELIVERED,
        FAILED
    }
}

