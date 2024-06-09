namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesAddDataRelationships
    {
        public ProfilesAddDataRelationshipsBundleId bundleId { get; set; } = new ProfilesAddDataRelationshipsBundleId();

        public ProfilesAddDataRelationshipsDevices devices { get; set; } = new ProfilesAddDataRelationshipsDevices();
        public ProfilesAddDataRelationshipsCertificates certificates { get; set; } = new ProfilesAddDataRelationshipsCertificates();

    }
}
