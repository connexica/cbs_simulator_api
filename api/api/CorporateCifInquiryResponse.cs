using Newtonsoft.Json;

namespace api;
public class CorporateCifInquiryResponse
{
    public class CustInqResponseData
    {
        public Getcorporatecustomerdetailsliteresponse getCorporateCustomerDetailsLiteResponse { get; set; } = new();
    }

    public class Getcorporatecustomerdetailsliteresponse
    {
        public Corporatecustomerdetailslite CorporateCustomerDetailsLite { get; set; } = new();
        public object getCorporateCustomerDetailsLite_CustomData { get; set; } = new();
    }

    public class Corporatecustomerdetailslite
    {
        public Corpdetlite corpDetLite { get; set; } = new();
        public List<Entitydocumentdetailslite> entityDocumentDetailsLite { get; set; } = [];
        public Financialdetlite financialDetLite { get; set; } = new();
        public Mappingdetailslite mappingDetailsLite { get; set; } = new();
        public Preferencesdetlite preferencesDetLite { get; set; } = new();
        public Corpbaseldetailslite corpBaselDetailsLite { get; set; } = new();
        public Tradefindtlslite tradeFinDtlsLite { get; set; } = new();
    }

    public class Corpdetlite
    {
        public string? average_annualincome { get; set; }
        public string? business_group { get; set; }
        public string? chargelevelcode { get; set; }
        public string? corp_key { get; set; }
        public string? corporate_name { get; set; }
        public string? corporatename_native { get; set; }
        public string? createdbysystemid { get; set; }
        public string? crncy_code { get; set; }
        public string? customer_rating { get; set; }
        public string? customer_rating_code { get; set; }
        public string? date_of_incorporation { get; set; }
        public string? defaultaddresstype { get; set; }
        public string? dsaid { get; set; }
        public string? entity_create_flg { get; set; }
        public string? entity_type { get; set; }
        public string? entityclass { get; set; }
        public string? keycontact_personname { get; set; }
        public string? legalentity_type { get; set; }
        public string? modifiedbysystemid { get; set; }
        public string? primary_service_center { get; set; }
        public string? primaryrm_id_code { get; set; }
        public string? primaryRMLogin_ID { get; set; }
        public string? principle_placeoperation { get; set; }
        public string? priority { get; set; }
        public string? region { get; set; }
        public string? registration_number { get; set; }
        public string? relationship_createdby { get; set; }
        public string? relationship_startdate { get; set; }
        public string? secondaryrm_id_code { get; set; }
        public string? secondRMLogin_ID { get; set; }
        public string? sector { get; set; }
        public string? sector_code { get; set; }
        public string? segment { get; set; }
        public string? short_name { get; set; }
        public string? short_name_native { get; set; }
        public string? source_of_funds { get; set; }
        public string? subsector { get; set; }
        public string? subsector_code { get; set; }
        public string? subsegment { get; set; }
        public string? taxid { get; set; }
        public string? tertiaryrmlogin_id { get; set; }
        public string? isEbankingEnabled { get; set; }
        public List<Corpaddresdetlite> corpaddresDetLite { get; set; } = [];
        [JsonConverter(typeof(SingleOrArrayConverter<Phoneemaildatalite>))]
        public List<Phoneemaildatalite> phoneEmailDataLite { get; set; } = [];
        public string? lastUpdateDate { get; set; }
        public string? documentReceived { get; set; }
        public string? corporateName { get; set; }
        public string? preferredPhoneType { get; set; }
        public string? preferredEmailType { get; set; }
        public string? Lang_Desc { get; set; }
        public string? NativeLangCode { get; set; }
        public string? CountryOfIncorporation { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? website_Address { get; set; }
        public string? health_Code { get; set; }
        public string? RiskProfileScore { get; set; }
        public string? Status_Desc { get; set; }
        public string? Cust_Type_Desc { get; set; }
        public string? Line_of_Activity_Desc { get; set; }
        public string? business_Type { get; set; }
        public string? Cust_Grp_Desc { get; set; }
        public string? preferredCalendar { get; set; }
        public string? Pref_code { get; set; }
        public string? delinquency_Flag { get; set; }
        public string? trade_Services_Availed { get; set; }
        public string? islamic_banking_customer { get; set; }
        public string? zakat_deduction { get; set; }
        public string? Cust_Swift_Code_Desc { get; set; }
        public string? Is_Swift_Code_of_Bank { get; set; }
        public string? Tds_CIF_Id { get; set; }
        public string? Tds_Tbl_Desc { get; set; }
        public string? asset_classification { get; set; }
        public string? Customer_Level_Provisioning { get; set; }
        public string? document_Received_Flag { get; set; }
        public string? remarks { get; set; }
        public string? phone { get; set; }
        public string? Email2 { get; set; }
        public string? secondaryRM_ID { get; set; }
        public string? primaryRM_ID { get; set; }
        public string? Health_Desc { get; set; }
        public string? Cust_hlth { get; set; }
        public string? ForeignTaxReportingCountry { get; set; }
        public string? FatcaRemarks { get; set; }
        public string? ForeignTaxReportingStatus { get; set; }
        public string? Tds_Tbl_Code { get; set; }
        public string? KYCDate { get; set; }
        public string? KYCReviewDate { get; set; }
        public string? submitForKYC { get; set; }
        public string? riskRating { get; set; }
    }

    public class Corpaddresdetlite
    {
        public string? address_line1 { get; set; }
        public string? address_line2 { get; set; }
        public string? address_line3 { get; set; }
        public string? addresscategory { get; set; }
        public string? building_level { get; set; }
        public string? businessCenter { get; set; }
        public string? cellno { get; set; }
        public string? cellnocitycode { get; set; }
        public string? cellnocountrycode { get; set; }
        public string? cellnolocalcode { get; set; }
        public string? city { get; set; }
        public string? cityCode { get; set; }
        public string? country { get; set; }
        public string? countryCode { get; set; }
        public string? domicile { get; set; }
        public string? end_date { get; set; }
        public string? faxno { get; set; }
        public string? faxno2 { get; set; }
        public string? faxnocitycode { get; set; }
        public string? faxnocitycode2 { get; set; }
        public string? faxnocountrycode { get; set; }
        public string? faxnocountrycode2 { get; set; }
        public string? faxnolocalcode { get; set; }
        public string? faxnolocalcode2 { get; set; }
        public string? freeTextAddress { get; set; }
        public string? freeTextLabel { get; set; }
        public string? holdMailFlag { get; set; }
        public string? holdMailInitiatedBy { get; set; }
        public string? holdMailReason { get; set; }
        public string? house_no { get; set; }
        public string? locality_name { get; set; }
        public string? mailstop { get; set; }
        public string? name { get; set; }
        public string? pagerno { get; set; }
        public string? pagernocitycode { get; set; }
        public string? pagernocountrycode { get; set; }
        public string? pagernolocalcode { get; set; }
        public string? preferredAddress { get; set; }
        public string? preferredFormat { get; set; }
        public string? premise_name { get; set; }
        public string? residentialstatus { get; set; }
        public string? salutation_code { get; set; }
        public string? start_date { get; set; }
        public string? state { get; set; }
        public string? street_name { get; set; }
        public string? street_no { get; set; }
        public string? suburb { get; set; }
        public string? telex { get; set; }
        public string? telexCityCode { get; set; }
        public string? telexCountryCode { get; set; }
        public string? telexLocalCode { get; set; }
        public string? town { get; set; }
        public string? zip { get; set; }
        public string? email { get; set; }
        public string? phoneNo1 { get; set; }
        public string? state_Code { get; set; }
        public string? isAddressVerified { get; set; }
        public string? lastUpdateDate { get; set; }
        public string? isAddressProofRcvd { get; set; }
        public string? swiftName { get; set; }
        public string? addressID { get; set; }
    }

    public class Phoneemaildatalite
    {
        public string? email { get; set; }
        public string? emailpalm { get; set; }
        public string? phoneemailtype { get; set; }
        public string? phoneno { get; set; }
        public string? phonenocitycode { get; set; }
        public string? phonenocountrycode { get; set; }
        public string? phonenolocalcode { get; set; }
        public string? phoneoremail { get; set; }
        public string? preferredflag { get; set; }
        public string? workextension { get; set; }
        public string? start_date { get; set; }
    }

    public class Financialdetlite
    {
        public string? business_assets { get; set; }
        public string? country { get; set; }
        public string? created_from { get; set; }
        public string? crncy_code { get; set; }
        public string? cust_fin_year_end_mnth { get; set; }
        public string? cust_net_worth { get; set; }
        public string? deposits_other_bank { get; set; }
        public string? entity_create_flg { get; set; }
        public string? entity_type { get; set; }
        public string? financial_year { get; set; }
        public string? financial_year_end { get; set; }
        public string? invest_shares_units { get; set; }
        public string? legal_details { get; set; }
        public string? legal_status { get; set; }
        public string? numberof_employees { get; set; }
        public string? period { get; set; }
        public string? property { get; set; }
        public string? sharehld_flg { get; set; }
        public string? tot_share_investment { get; set; }
        public string? total_amount_held { get; set; }
        public string? type_of_statement { get; set; }
    }

    public class Mappingdetailslite
    {
        public string? cifIdInProductProcessor { get; set; }
        public string? productProcessorId { get; set; }
    }

    public class Preferencesdetlite
    {
        public string? calendar_type { get; set; }
        public string? combined_stmt_reqd { get; set; }
        public string? combined_stmtflag { get; set; }
        public string? cust_floor_limit_tds { get; set; }
        public string? description { get; set; }
        public string? do_not_email { get; set; }
        public string? do_not_mail { get; set; }
        public string? do_not_phone { get; set; }
        public string? entity_create_flg { get; set; }
        public string? entity_type { get; set; }
        public string? frequency { get; set; }
        public string? frequency_holidaystatus { get; set; }
        public string? frequency_weekday { get; set; }
        public string? holdmail_flag { get; set; }
        public string? loansstatementtype { get; set; }
        public string? mode_of_dispatch { get; set; }
        public string? preferred_channel { get; set; }
        public string? preffered_channel_code { get; set; }
        public string? statement_type { get; set; }
        public string? tds_exmpt_ref_num { get; set; }
        public string? tds_exmpt_rmks { get; set; }
        public string? tdsstatementtype { get; set; }
        public Prefcorpmiscinfolite prefCorpmiscInfoLite { get; set; } = new();
    }

    public class Prefcorpmiscinfolite
    {
        public string? amount1 { get; set; }
        public string? amount2 { get; set; }
        public string? amount3 { get; set; }
        public string? amount4 { get; set; }
        public string? date1 { get; set; }
        public string? int1 { get; set; }
        public string? str1 { get; set; }
        public string? str10 { get; set; }
        public string? str11 { get; set; }
        public string? str2 { get; set; }
        public string? str3 { get; set; }
        public string? str4 { get; set; }
        public string? str5 { get; set; }
        public string? str6 { get; set; }
        public string? str7 { get; set; }
        public string? str8 { get; set; }
        public string? str9 { get; set; }
        public string? type { get; set; }
        public string? creditDiscountPcnt { get; set; }
        public string? debitDiscountPcnt { get; set; }
        public string? withholdingTaxPcnt { get; set; }
    }

    public class Corpbaseldetailslite
    {
        public string? obligor { get; set; }
        public string? pdriskrating { get; set; }
        public string? lossdefprcnt { get; set; }
        public string? probdefprcnt { get; set; }
        public string? regulatoryrating { get; set; }
        public string? seniority { get; set; }
        public string? exposurecategory { get; set; }
        public string? wholesaleexposubcat { get; set; }
        public string? revenuesize { get; set; }
        public string? totalassets { get; set; }
        public string? custmessage { get; set; }
        public string? iscustnamedisclosure { get; set; }
        public string? securityindicator { get; set; }
        public string? isconglomerate { get; set; }
        public string? investment { get; set; }
        public string? industrycode { get; set; }
    }

    public class Tradefindtlslite
    {
        public string? CautionStat { get; set; }
        public string? CentralBankId { get; set; }
        public string? ContractLimit { get; set; }
        public string? CorpName { get; set; }
        public string? CustNative { get; set; }
        public string? DCMmarginPercentage { get; set; }
        public string? DCNextNumCode { get; set; }
        public string? DCSanctionIngAuth { get; set; }
        public string? ExpImpInd { get; set; }
        public string? FaxNum { get; set; }
        public string? FCSanctionIngAuth { get; set; }
        public string? HundredPercentFlag { get; set; }
        public string? InlandTradeAllowed { get; set; }
        public string? LeasingLiabilities { get; set; }
        public string? PartyType { get; set; }
        public string? Phone { get; set; }
        public string? ProductionCycle { get; set; }
        public string? Rmks { get; set; }
        public string? SpecialCustFlag { get; set; }
        public string? SSIFlag { get; set; }
        public string? Telex { get; set; }
        public string? TradeAuthorityCode { get; set; }
        public string? CustomerIndicator { get; set; }
        public string? cifID { get; set; }
        public string? PartyConstitution { get; set; }
        public string? TradeCCY { get; set; }
        public string? AuthorizedSignature1 { get; set; }
        public string? AuthorizedSignature2 { get; set; }
        public string? AuthorizedSignature3 { get; set; }
    }

    public class Entitydocumentdetailslite
    {
        public string? countryofissue { get; set; }
        public string? doccode { get; set; }
        public string? docdelflg { get; set; }
        public string? docissuedate { get; set; }
        public string? docremarks { get; set; }
        public string? doctypecode { get; set; }
        public string? doctypedescr { get; set; }
        public string? entitytype { get; set; }
        public string? identificationtype { get; set; }
        public string? ismandatory { get; set; }
        public string? placeofissue { get; set; }
        public string? referencenumber { get; set; }
        public string? scanrequired { get; set; }
        public string? status { get; set; }
        public string? isDocumentVerified { get; set; }
        public string? IDIssuedOrganisation { get; set; }
        public string? entityDocumentID { get; set; }
    }
}