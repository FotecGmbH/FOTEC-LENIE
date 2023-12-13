// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model.Report;

namespace Database.Converter
{
    /// <summary>
    ///     <para>DB Convert für Reports</para>
    ///     Klasse ConverterDbReport. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbReport
    {
        /// <summary>
        ///     Umwandeln in ExItem
        /// </summary>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExReport ToExReport(this TableIdeaReport dbItem)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExReport
                         {
                             IdeaId = dbItem.TblIdeaId,
                             Reason = dbItem.Reason,
                             UserId = dbItem.TblUserId,
                         };

            if (dbItem.TblIdea != null!)
            {
                exItem.IdeaTitle = dbItem.TblIdea.Title;
            }

            return exItem;
        }

        /// <summary>
        /// </summary>
        /// <param name="exItem"></param>
        /// <param name="dbItem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableReport(this ExReport exItem, TableIdeaReport dbItem)
        {
            if (exItem == null)
            {
                throw new ArgumentNullException(nameof(exItem));
            }

            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            dbItem.Reason = exItem.Reason;
        }
    }
}