using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class IdValidationResponseDTO
    {
        public string identifier { get; set; }
        public string identifierName { get; set; }
        public string name { get; set; }
        public string orgName { get; set; }
        public string applicationName { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string status { get; set; }
        public string kycMethod { get; set; }
        public string signedResponse { get; set; }
        public string nationality { get; set; }
        public string photo { get; set; }
        public string deviceId { get; set; }
        public string datetime { get; set; }
        public string validationDateTime { get; set; }
        public string expiryDate { get; set; }
        public string issueDate { get; set; }
        public string documentStatus { get; set; }
        public CallStackRequestDTO request { get; set; }
    }


    public class VerifiedIdValidationResponseDTO
    {
        public string name { get; set; }
        public string nationality { get; set; }
        public string idNumber { get; set; }
        public string issueDate { get; set; }
        public string expiryDate { get; set; }
        public string photo { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string documentStatus { get; set; }
    }

    public class CallStackDTO
    {
        public CallStackRequestDTO request { get; set; }
        public object response { get; set; }
    }

    public class CallStackRequestDTO
    {
        public string idNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string expiryDate { get; set; }
        public string name { get; set; }
        public string cardNumber { get; set; }
        public string faceImage { get; set; }
        public string capturedFingerImage { get; set; }
        public string passportImage { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public int? fingerIndex { get; set; }
        public string customerPhoneNumber { get; set; }
        public string customerEmail { get; set; }
        public string remoteVerificationdata { get; set; }
        public bool? manualEntry { get; set; }
        public bool? authentication { get; set; }
        public string documentType { get; set; }
        public string documentNumber { get; set; }
        public string documentNationality { get; set; }
    }

    public class IdValidationCallStackResponseDTO
    {
        public string idNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string expiryDate { get; set; }
        public string issueDate { get; set; }
        public string name { get; set; }
        public string cardNumber { get; set; }
        public string cardStatus { get; set; }
        public string reason { get; set; }
        public string nationality { get; set; }
        public string signedResponse { get; set; }
        public string signedTimeStamp { get; set; }
        public string documentStatus { get; set; }
        public IdCardData idCardData { get; set; }
    }

    public class IdCardData
    {
        public IdNumber idNumber { get; set; }
        public NonModifiableData nonModifiableData { get; set; }
        public ModifiableData modifiableData { get; set; }
        public HomeAddress homeAddress { get; set; }
        public WorkAddress workAddress { get; set; }
        public Photography photography { get; set; }
        public HolderSignatureImage holderSignatureImage { get; set; }
    }

    public class IdNumber
    {
        public string idNumber { get; set; }
        public string cardNumber { get; set; }
    }

    public class NonModifiableData
    {
        public string idType { get; set; }
        public string issueDate { get; set; }
        public string expiryDate { get; set; }
        public string titleArabic { get; set; }
        public string fullNameArabic { get; set; }
        public string titleEnglish { get; set; }
        public string fullNameEnglish { get; set; }
        public string gender { get; set; }
        public string nationalityArabic { get; set; }
        public string nationalityEnglish { get; set; }
        public string nationalityCode { get; set; }
        public string dateOfBirth { get; set; }
        public string placeOfBirthArabic { get; set; }
        public string placeOfBirthEnglish { get; set; }
        public string dataSignature { get; set; }
    }

    public class ModifiableData
    {
        public string occupationCode { get; set; }
        public string occupationArabic { get; set; }
        public string occupationEnglish { get; set; }
        public string familyId { get; set; }
        public string occupationTypeArabic { get; set; }
        public string occupationTypeEnglish { get; set; }
        public string occupationFieldCode { get; set; }
        public string companyNameArabic { get; set; }
        public string companyNameEnglish { get; set; }
        public string maritalStatusCode { get; set; }
        public string husbandIdNumber { get; set; }
        public string sponsorTypeCode { get; set; }
        public string sponsorUnifiedNumber { get; set; }
        public string sponsorName { get; set; }
        public string residencyTypeCode { get; set; }
        public string residencyNumber { get; set; }
        public string residencyExpiryDate { get; set; }
        public string passportNumber { get; set; }
        public string passportTypeCode { get; set; }
        public string passportCountryCode { get; set; }
        public string passportCountryArabic { get; set; }
        public string passportCountryEnglish { get; set; }
        public string passportIssueDate { get; set; }
        public string passportExpiryDate { get; set; }
        public string qualificationLevelCode { get; set; }
        public string qualificationLevelArabic { get; set; }
        public string qualificationLevelEnglish { get; set; }
        public string degreeDescriptionArabic { get; set; }
        public string degreeDescriptionEnglish { get; set; }
        public string fieldOfStudyCode { get; set; }
        public string fieldOfStudyArabic { get; set; }
        public string fieldOfStudyEnglish { get; set; }
        public string placeOfStudyArabic { get; set; }
        public string placeOfStudyEnglish { get; set; }
        public string dateOfGraduation { get; set; }
        public string motherFullNameArabic { get; set; }
        public string motherFullNameEnglish { get; set; }
        public string dataSignature { get; set; }
    }

    public class HomeAddress
    {
        public string addressTypeCode { get; set; }
        public string locationCode { get; set; }
        public string emiratesCode { get; set; }
        public string emiratesDescArabic { get; set; }
        public string emiratesDescEnglish { get; set; }
        public string cityCode { get; set; }
        public string cityDescArabic { get; set; }
        public string cityDescEnglish { get; set; }
        public string streetArabic { get; set; }
        public string streetEnglish { get; set; }
        public string pobox { get; set; }
        public string areaCode { get; set; }
        public string areaDescArabic { get; set; }
        public string areaDescEnglish { get; set; }
        public string buildingNameArabic { get; set; }
        public string buildingNameEnglish { get; set; }
        public string flatNo { get; set; }
        public string residentPhoneNumber { get; set; }
        public string mobilePhoneNumber { get; set; }
        public string email { get; set; }
        public string dataSignature { get; set; }
    }

    public class WorkAddress
    {
        public string addressTypeCode { get; set; }
        public string locationCode { get; set; }
        public string companyNameArabic { get; set; }
        public string companyNameEnglish { get; set; }
        public string emiratesCode { get; set; }
        public string emiratesDescArabic { get; set; }
        public string emiratesDescEnglish { get; set; }
        public string cityCode { get; set; }
        public string cityDescArabic { get; set; }
        public string cityDescEnglish { get; set; }
        public string pobox { get; set; }
        public string streetArabic { get; set; }
        public string streetEnglish { get; set; }
        public string areaCode { get; set; }
        public string areaDescArabic { get; set; }
        public string areaDescEnglish { get; set; }
        public string buildingNameArabic { get; set; }
        public string buildingNameEnglish { get; set; }
        public string landPhoneNumber { get; set; }
        public string mobilePhoneNumber { get; set; }
        public string email { get; set; }
        public string dataSignature { get; set; }
    }

    public class Photography
    {
        public string cardHolderPhoto { get; set; }
        public string dataSignature { get; set; }
    }

    public class HolderSignatureImage
    {
        public string holderSignatureImage { get; set; }
        public string dataSignature { get; set; }
    }




    //public class EmiratesIdResponse
    //{
    //    [JsonProperty("code")]
    //    public int Code { get; set; }

    //    [JsonProperty("data")]
    //    public PersonData Data { get; set; }
    //}

    public class EmiratesIdResponse
    {
        public string IdNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string ExpiryDate { get; set; }
        public string Name { get; set; }
        public string CardNumber { get; set; }
        public string CardStatus { get; set; }
        public string MatchStatus { get; set; }
        public string Reason { get; set; }
        public PersonData Data { get; set; }
        public string SignedResponse { get; set; }
        public string SignedTimeStamp { get; set; }
        public string Nationality { get; set; }
        public string PassportStatus { get; set; }
        public string DocumentStatus { get; set; }
    }


    public class PersonData
    {
        [JsonProperty("personNo")]
        public int PersonNo { get; set; }

        [JsonProperty("currentNationality")]
        public CodeDescription CurrentNationality { get; set; }

        [JsonProperty("tribe")]
        public Tribe Tribe { get; set; }

        [JsonProperty("fullNameAr")]
        public string FullNameAr { get; set; }

        [JsonProperty("firstNameEn")]
        public string FirstNameEn { get; set; }

        [JsonProperty("firstNameAr")]
        public string FirstNameAr { get; set; }

        [JsonProperty("secondNameEn")]
        public string SecondNameEn { get; set; }

        [JsonProperty("secondNameAr")]
        public string SecondNameAr { get; set; }

        [JsonProperty("thirdNameEn")]
        public string ThirdNameEn { get; set; }

        [JsonProperty("thirdNameAr")]
        public string ThirdNameAr { get; set; }

        [JsonProperty("fourthNameEn")]
        public string FourthNameEn { get; set; }

        [JsonProperty("fourthNameAr")]
        public string FourthNameAr { get; set; }

        [JsonProperty("fifthNameAr")]
        public string FifthNameAr { get; set; }

        [JsonProperty("motherNameAr")]
        public string MotherNameAr { get; set; }

        [JsonProperty("motherFatherNameAr")]
        public string MotherFatherNameAr { get; set; }

        [JsonProperty("fullNameEn")]
        public string FullNameEn { get; set; }

        [JsonProperty("fifthNameEn")]
        public string FifthNameEn { get; set; }

        [JsonProperty("motherNameEn")]
        public string MotherNameEn { get; set; }

        [JsonProperty("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("personClass")]
        public CodeDescription PersonClass { get; set; }

        [JsonProperty("gender")]
        public CodeDescription Gender { get; set; }

        [JsonProperty("maritalStatus")]
        public CodeDescription MaritalStatus { get; set; }

        [JsonProperty("placeOfBirthAr")]
        public string PlaceOfBirthAr { get; set; }

        [JsonProperty("emiratesId")]
        public string EmiratesId { get; set; }

        [JsonProperty("placeOfBirthEn")]
        public string PlaceOfBirthEn { get; set; }

        [JsonProperty("hijriDateOfBirth")]
        public string HijriDateOfBirth { get; set; }

        [JsonProperty("mobileNo")]
        public string MobileNo { get; set; }

        [JsonProperty("binaryObjects")]
        public List<BinaryObject> BinaryObjects { get; set; }

        [JsonProperty("activePassport")]
        public Passport ActivePassport { get; set; }

        [JsonProperty("emiratesIdDetail")]
        public EmiratesIdDetail EmiratesIdDetail { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("occupation")]
        public CodeDescription Occupation { get; set; }

        [JsonProperty("localAddress")]
        public LocalAddress LocalAddress { get; set; }

        [JsonProperty("familyBook")]
        public FamilyBook FamilyBook { get; set; }

        [JsonProperty("immigrationFile")]
        public ImmigrationFile ImmigrationFile { get; set; }

        [JsonProperty("immigrationStatus")]
        public CodeDescription ImmigrationStatus { get; set; }

        [JsonProperty("sponsor")]
        public Sponsor Sponsor { get; set; }

        [JsonProperty("isInsideUAE")]
        public bool IsInsideUAE { get; set; }

        [JsonProperty("travelDetail")]
        public TravelDetail TravelDetail { get; set; }

        [JsonProperty("personClassification")]
        public string PersonClassification { get; set; }
    }

    public class Sponsor
    {
        [JsonProperty("nameAr")]
        public string NameAr { get; set; }

        [JsonProperty("nameEn")]
        public string NameEn { get; set; }

        [JsonProperty("department")]
        public CodeDescription Department { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("localAddress")]
        public LocalAddress LocalAddress { get; set; }

        [JsonProperty("type")]
        public CodeDescription Type { get; set; }

        [JsonProperty("sponsorNationality")]
        public CodeDescription SponsorNationality { get; set; }
    }

    public class CodeDescription
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("descriptionAr", NullValueHandling = NullValueHandling.Ignore)]
        public string DescriptionAr { get; set; }

        [JsonProperty("descriptionEn", NullValueHandling = NullValueHandling.Ignore)]
        public string DescriptionEn { get; set; }

        [JsonProperty("abbreviation", NullValueHandling = NullValueHandling.Ignore)]
        public string Abbreviation { get; set; }
    }

    public class Tribe { }

    public class BinaryObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("binaryDocType")]
        public string BinaryDocType { get; set; }

        [JsonProperty("binaryBase64String")]
        public string BinaryBase64String { get; set; }

        [JsonProperty("binaryCapturedByUser")]
        public string BinaryCapturedByUser { get; set; }

        [JsonProperty("ouUnit")]
        public string OuUnit { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }
    }

    public class Passport
    {
        [JsonProperty("documentType")]
        public CodeDescription DocumentType { get; set; }

        [JsonProperty("documentNo")]
        public string DocumentNo { get; set; }

        [JsonProperty("documentNationality")]
        public CodeDescription DocumentNationality { get; set; }

        [JsonProperty("documentIssueCountry")]
        public CodeDescription DocumentIssueCountry { get; set; }

        [JsonProperty("issueDate")]
        public string IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }
    }

    public class EmiratesIdDetail
    {
        [JsonProperty("documentType")]
        public CodeDescription DocumentType { get; set; }

        [JsonProperty("documentNo")]
        public string DocumentNo { get; set; }

        [JsonProperty("issueDate")]
        public string IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }
    }

    public class Title { }

    public class LocalAddress
    {
        [JsonProperty("emirate")]
        public CodeDescription Emirate { get; set; }

        [JsonProperty("city")]
        public CodeDescription City { get; set; }

        [JsonProperty("area")]
        public CodeDescription Area { get; set; }

        [JsonProperty("street")]
        public Street Street { get; set; }

        [JsonProperty("poBox")]
        public string PoBox { get; set; }

        [JsonProperty("mobileNo")]
        public string MobileNo { get; set; }

        [JsonProperty("homePhone")]
        public string HomePhone { get; set; }

        [JsonProperty("workPhone")]
        public string WorkPhone { get; set; }
    }

    public class Street
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("descriptionAr")]
        public string DescriptionAr { get; set; }
    }

    public class FamilyBook
    {
        [JsonProperty("khulasitQaidNo")]
        public string KhulasitQaidNo { get; set; }

        [JsonProperty("familyBookNo")]
        public string FamilyBookNo { get; set; }

        [JsonProperty("familyBookStartDate")]
        public string FamilyBookStartDate { get; set; }

        [JsonProperty("familyBookRelation")]
        public string FamilyBookRelation { get; set; }
    }

    public class ImmigrationFile
    {
        [JsonProperty("fileType")]
        public CodeDescription FileType { get; set; }

        [JsonProperty("fileNumber")]
        public string FileNumber { get; set; }

        [JsonProperty("issuePlace")]
        public string IssuePlace { get; set; }

        [JsonProperty("issueDate")]
        public string IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }
    }

    public class TravelDetail
    {
        [JsonProperty("isInside")]
        public bool IsInside { get; set; }

        [JsonProperty("personNo")]
        public int PersonNo { get; set; }

        [JsonProperty("eboCode")]
        public int EboCode { get; set; }

        [JsonProperty("sdeCode")]
        public CodeDescription SdeCode { get; set; }

        [JsonProperty("travelType")]
        public CodeDescription TravelType { get; set; }

        [JsonProperty("travelDate")]
        public DateTime TravelDate { get; set; }

        [JsonProperty("travelDocumentNo")]
        public string TravelDocumentNo { get; set; }

        [JsonProperty("travelDocumentIssueDate")]
        public string TravelDocumentIssueDate { get; set; }

        [JsonProperty("travelDocumentExpiryDate")]
        public string TravelDocumentExpiryDate { get; set; }
    }


    public class IdValidationSummaryResponse
    {
        public int TotalKycCountSuccessfulCurrentMonth { get; set; }
        public string OrgName { get; set; }
        public string OrgLogo { get; set; }
        public string OrgId { get; set; }
        public string SpocEmail { get; set; }
        public string LastKycFailedTimestamp { get; set; }
        public DateTime? LastKycFailedTimestampUTC { get; set; }
        public int TotalKycCountFailedCurrentYear { get; set; }
        public string LastKycSuccessfulTimestamp { get; set; }
        public DateTime? LastKycSuccessfulTimestampUTC { get; set; }
        public int TotalKycCountFailed { get; set; }
        public List<string> KycMethods { get; set; }
        public int TotalKycCountSuccessful { get; set; }
        public int TotalKycCountSuccessfulCurrentYear { get; set; }
        public int TotalKycCountFailedCurrentMonth { get; set; }
        public string LastKycSucessfulTimestampDate { get; set; }
        public DateTime? LastKycSucessfulTimestampDateUTC { get; set; }
        public string OrgStatus { get; set; }
    }

    public class OrgKycRecord
    {
        public string OrgId { get; set; }
        public string KycMethods { get; set; } // Stored as a JSON array string
    }

    public class KycLogResponseDTO
    {
        [JsonProperty("result")]
        public List<LogReportDTO> Result { get; set; }

        [JsonProperty("perPage")]
        public int PerPage { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class KycSummaryDTO
    {
        public int TotalFailedKycs { get; set; }
        public int TotalSuccessfulKycsThisMonth { get; set; }
        public int TotalSuccessfulKycs { get; set; }
        public int TotalServiceProviders { get; set; }
        public int NewServiceProvidersThisMonth { get; set; }
        public int TotalKycDevices { get; set; }
    }

    public class OrgKycSummaryDTO
    {
        public string orgName { get; set; }
        public string spocEmail { get; set; }
        public string orgLogo { get; set; }
        public string orgId {   get; set; }
        public List<string> KycMethods { get; set; }
        public List<string> KycDevices { get; set; }
        public string spocName { get; set; }
        public string spocMobileNumber { get; set; }
        public Dictionary<string, MonthlyStat> monthlyStats { get; set; }
        public int totalKycCountSuccessful { get; set; }

        public int totalKycCountFailed { get; set; }

        public int totalKycDone { get; set; }

        public int totalKycCountSuccessfulCurrentMonth { get; set; }

        public int totalKycCountFailedCurrentMonth { get; set; }

        public int totalKycDoneCurrentMonth { get; set; }

        public int totalKycCountSuccessfulCurrentYear { get; set; }

        public int totalKycCountFailedCurrentYear { get; set; }

        public string lastKycSuccessfulTimestamp { get; set; }
        public int TotalKycDevices { get; set; }
    }

    public class MonthlyStat
    {
        public int successCount { get; set; }
        public int failedCount { get; set; }
    }


    public class ServiceProviderPageViewModel
    {
        public List<IdValidationResponseDTO> Reports { get; set; }
        public OrgKycSummaryDTO OrgData { get; set; }

        public int TotalCount { get; set; }
    }

}
