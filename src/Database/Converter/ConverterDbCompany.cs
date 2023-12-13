// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model;
using Exchange.Model.Organization;


namespace Database.Converter;

/// <summary>
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbCompany
{
    /// <summary>
    ///     TableOrganization zu ExOrganization
    /// </summary>
    /// <param name="org"></param>
    /// <exception cref="ArgumentNullException">Wenn tblCompany null ist</exception>
    /// <returns></returns>
    public static ExOrganization ToExExOrganization(this TableOrganization org)
    {
        if (org == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToExExOrganization)}): {nameof(org)}");
        }

        var r = new ExOrganization
                {
                    OrganizationId = org.Id,
                    OrganizationType = org.OrganizationType,
                    Name = org.Name.Trim(),
                    PostalCode = org.PostalCode.Trim(),
                };

        if (org.TblPermissions != null!)
        {
            r.UsersCount = org.TblPermissions.Count;
        }

        if (org.TblIdeaOrganisations != null!)
        {
            r.IdeasCount = org.TblIdeaOrganisations.Count;
        }

        if (org.TblAccessToken != null!)
        {
            foreach (var token in org.TblAccessToken)
            {
                r.Tokens.Add(new ExAccessToken
                             {
                                 DbId = token.Id,
                                 GuiltyUntilUtc = token.GuiltyUntilUtc,
                                 Token = token.Token
                             });
            }
        }

        return r;
    }

    /// <summary>
    ///     ExOrganization nach TableOrganization
    /// </summary>
    /// <param name="o">ExOrganization</param>
    /// <param name="t">TableOrganization</param>
    public static void ToTableOrganization(this ExOrganization o, TableOrganization t)
    {
        if (o == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToTableOrganization)}): {nameof(o)}");
        }

        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbCompany)}]({nameof(ToTableOrganization)}): {nameof(t)}");
        }

        t.OrganizationType = o.OrganizationType;
        t.Name = o.Name.Trim();
        t.PostalCode = o.PostalCode.Trim();
    }
}