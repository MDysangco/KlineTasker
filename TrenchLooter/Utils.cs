using Microsoft.Data.SqlClient;
using System.Data;

namespace TrenchLooter
{

    public static class DataReaderMapper
    {
        public static T MapTo<T>(this SqlDataReader reader) where T : new()
        {
            T obj = new T();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                if (!reader.HasColumn(prop.Name))
                    continue;

                int ordinal = reader.GetOrdinal(prop.Name);

                if (reader.IsDBNull(ordinal))
                {
                    prop.SetValue(obj, null);
                    continue;
                }

                object value = reader.GetValue(ordinal);

                if (prop.PropertyType.IsEnum || (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum ?? false))
                {
                    var enumType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var intValue = Convert.ToInt32(value);
                    var enumValue = Enum.ToObject(enumType, intValue);
                    prop.SetValue(obj, enumValue);
                    continue;
                }


                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                prop.SetValue(obj, Convert.ChangeType(value, targetType));
            }

            return obj;
        }

        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}
