/*
BSD 3-Clause License

Copyright (c) 2024, Jooty

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
*/
using System.Security.Cryptography;
using System.Text;
using Imcodec.Cryptography;

namespace Imcodec.Test.CryptographyTest;

public class RsaKeySlotTest {

    private const string TEST_PRIVATE_KEY = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDHZq/ZjEdb8mvZmF/AgVYT4ptgL1yoKsUzmqGZKytGNHTQ8zWYZXea73hjYReV52uzIqs+17GXJU9Ix19+aXwRiAOnAeN48wQsOnFtKfRw8mCaURzK6N01L0kVrEL8V5x0knu3eh0rIU+DKn0VXEAFrfDFLD2oP2a3dNbvf25wI5umIVOX7AG8Q+BoT5UuR0fqkCsjRkVVeBVTJOhhDMatSB2DOZP11ma/rBd9G84Ho6B9kMKyZVz0CDuJ0yxOGQJMfWsVNnreBOq1z6sZEECtbppfgpjmBSmWdxitfNh1cm1hFPAr4mDqEkaPYiF7M2uVsarGkk8UZdeBtHAASQxHAgMBAAECggEAINZk/eXqF9bzsoveCO0ZEyvtpoi7hj4055X+0s3sfHfwXOYQh3Oo9MJHCHwEJFL4xjFeviSglww1OFO3rYZgQOAt/yItJyKY0f0gkiJR97USpOyCr1prQ3JGuOOmIbmpMaawlRCZHnHdOZ6U/CA1suRoPYWsSbmB3ZMc6lK0CedTRwnGVLudcTf4mOaR0XIUQxB2TsQCHj7u+bSFyoILSOMw9kOUhZLetZPB/Nvw3plD/+E+SSz54/u3gIGHvpdN658w5vSUur6mMDFkafU/Lw0bhxSu85VY3Zt2+mITybNvUZe2b611luVpKQQloJo1ZH+HDOzwRFCVxeaashNz8QKBgQD1vTEB0Boh5zIiZJNIPSjyJao4WGP3zlxpnihMsB9L5nkBdEyk/DUqPwcTFdAn0P/1hmA15tD+dGB1kOCO+BmEqhYG/J4PvxRSKCMm6CKTO70eue+xmk15ayfX3Lm0+OuCPLzRQ8TglcYWeGg/9hv8apOPDOald9HaO4COP0XVCwKBgQDPuiuB52ZrYUBhN8PTjzCTGOWJThw8vnH3T4JudOuiZfEcj7ofcmTWyAe3vgROeA4r4RnKdPDRySoaKBweSi0txYOzNMFRoVGSbfZptB9yO36ZIkmf1F5A+s1545IdFlxLJrUiIZlF1/ddTi49L+TVBTUUbLU7ner1vFGAbGBzNQKBgC6p/41llCDOA2IukR1Np+HbryhF+K3M/RV9WA+//iFuoZnaGNfXoMzl7RT9tmppMMHSA6EYtKUtOkf4+/uKN2ZO2iH3hJhe45/6STqRKEenN3GHQ3IJaB7fb1B5dLZd8nLnz785TECNkHEh6kIDHStgnaazf3T6fNohJHdeoocjAoGBAIF2iRkcK/Pul54/kBexXVfAI4aQnP6qvwKglizbR+Rd3oirdxeURw6pek3k6nAxWLu2Pkm02kYVNt04dFXYaxHk0LEGxxlmDh3R1MqKTTL3q24rNXGcfOXIqfk2gOEKB5l61VchU5J5CB0p1Z2i1N6CsVM6cTVntYFTzgPZB7+lAoGBAMYeHUP9mbivd+ueZMfm0kTnfCir4mMfCkDDthZNcZkZLSJ2PSjf+nCaL3ofGzrggPE5dhuiM6LRd/eNMBTUq5KOwQHHJllwc3krO5HXtl1dyFOzgNBZAdMCMVE3EXkmAiRlM0bu29lva7YTHWGRcD5lrvO13dNG21Aa2sDYPp7V";
    private const string TEST_PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAx2av2YxHW/Jr2ZhfwIFWE+KbYC9cqCrFM5qhmSsrRjR00PM1mGV3mu94Y2EXledrsyKrPtexlyVPSMdffml8EYgDpwHjePMELDpxbSn0cPJgmlEcyujdNS9JFaxC/FecdJJ7t3odKyFPgyp9FVxABa3wxSw9qD9mt3TW739ucCObpiFTl+wBvEPgaE+VLkdH6pArI0ZFVXgVUyToYQzGrUgdgzmT9dZmv6wXfRvOB6OgfZDCsmVc9Ag7idMsThkCTH1rFTZ63gTqtc+rGRBArW6aX4KY5gUplncYrXzYdXJtYRTwK+Jg6hJGj2IhezNrlbGqxpJPFGXXgbRwAEkMRwIDAQAB";
    private const string REFERENCE_SIGNATURE = "TF14+xL6V9W2NSb6Q1jPgKSv+D5ntcDdLwiUnhkm1JYjCyjefPzG4Jy7QyrEpq1SEgXpU0AQDKP8biKNm9U6R3m9BmE02RFXNRQs9skQfd59InRscZRNDBwY4Dsf+Lc9ExiPvAQhh8j35/3NBorNA/ZqXYPAe9d1NfMDEMZ7Zb2Ao1OXnLM7XhP38HRNQDyPRtobByZ6ZpDKwnZUB8SZT+X+onQ2IKbZH1cVy8c2bHJP8M6rRnZpKj+lVyS4TQePuBWaJZTI8jYI7/v4yLirgKaA800bS+czgL2JO59pKlyW7Er1xtTy3EFT651er8WEPQDykRSMVcE+XNirE+cXng==";
    private const string REFERENCE_OAEP_CIPHERTEXT = "ODv2EwcWWFPOF2Ev5Zc2X1QsVZYxe+CToSHg3Dw4Pf85t7ePv6t5n/xed60bxlTgs6j2Ib9hcwPaytvK6UeCYcI5nDn9WMR4j2NLBegTQ3LNqtOA4u60mt92OtMbldbFtHJck4FdUe4caa1WfHO1JL9rJ1Yxw9+Uj65c/I3dXFFFrsP3BfLsf2KS7r/rTY+bSzHOT+hw3EfXUOT8+TYEwp7fIKIq9HJN1e3O/K7G0dC7wE1gu+hyjsirdaaVGI0HLBEzudy9NQUjRkj/Q9wZVbcpy0Fiu0gDvrj8t3+tUuVdl7xwihYlW0TwPcGGc62wGPmteHNeTxcIsJnbvpwEmA==";
    private const string PAYLOAD = "synthetic payload";

    [Fact]
    public void Sign_MatchesReferenceImplementation() {
        using var slot = RsaKeySlot.FromPrivateKey(Convert.FromBase64String(TEST_PRIVATE_KEY));

        var signature = slot.Sign(Encoding.ASCII.GetBytes(PAYLOAD));

        Assert.Equal(REFERENCE_SIGNATURE, Convert.ToBase64String(signature));
    }

    [Fact]
    public void Verify_AcceptsReferenceSignatureWithPublicKeyOnly() {
        using var slot = RsaKeySlot.FromPublicKey(Convert.FromBase64String(TEST_PUBLIC_KEY));

        var payload = Encoding.ASCII.GetBytes(PAYLOAD);

        Assert.True(slot.Verify(payload, Convert.FromBase64String(REFERENCE_SIGNATURE)));
        Assert.False(slot.Verify(Encoding.ASCII.GetBytes("other payload"), Convert.FromBase64String(REFERENCE_SIGNATURE)));
    }

    [Fact]
    public void Decrypt_ReadsReferenceOaepCiphertext() {
        using var slot = RsaKeySlot.FromPrivateKey(Convert.FromBase64String(TEST_PRIVATE_KEY));

        var plain = slot.Decrypt(Convert.FromBase64String(REFERENCE_OAEP_CIPHERTEXT));

        Assert.Equal(PAYLOAD, Encoding.ASCII.GetString(plain));
    }

    [Fact]
    public void GeneratedKey_RoundTripsEncryptAndSign() {
        using var slot = RsaKeySlot.Generate();
        using var publicOnly = RsaKeySlot.FromPublicKey(slot.ExportPublicKey());
        var payload = Encoding.ASCII.GetBytes(PAYLOAD);

        Assert.Equal(payload, slot.Decrypt(publicOnly.Encrypt(payload)));
        Assert.True(publicOnly.Verify(payload, slot.Sign(payload)));
    }
}
