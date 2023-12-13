// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using Database.Tables;
using Exchange.Model.FutureWishes;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Zukünftige Features</para>
    ///     Klasse ConverterDbFutureWish. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbFutureWish
    {
        /// <summary>
        ///     Umwandeln für Exchangeklasse
        /// </summary>
        /// <param name="dbItem">Element aus DB</param>
        /// <param name="userId">optional UserId für Liked Ja/Nein</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExFutureWish ToExFutureWish(this TableFutureWish dbItem, long? userId = null)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExFutureWish
                         {
                             Description = dbItem.Description,
                             Title = dbItem.Title,
                             Link = dbItem.Link,
                         };

            if (userId != null && dbItem.TblFutureWishLikes != null!)
            {
                exItem.Liked = dbItem.TblFutureWishLikes.Any(x => x.TblUserId == userId);
            }

            return exItem;
        }
    }
}