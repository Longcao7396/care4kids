using System;
using System.Reflection;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Optional/optional helper that bumps an UpdatedAt field if it exists.
    /// Used by entities that don't natively expose UpdatedAt (e.g. Invitation).
    /// </summary>
    public static class EntityExtensions
    {
        public static void UpdatedAtSafe(this object entity)
        {
            if (entity == null) return;
            var prop = entity.GetType().GetProperty("UpdatedAt",
                BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
                prop.SetValue(entity, DateTime.Now);
        }
    }
}
