# Individual Existing Customer Verification — Simulator Test Scenarios

Prepared 2026-08-08. All accounts/CIFs below are simulator-only test data — none represent real
customers. Dispatch is by 11-digit Account Number (or 8-digit CIF for `RetailCIFInquiry`, and Account
Number again for `AccountFreezeInquiry`), decoded from the request's Base64 `Data` field by
`ConnectController`. Any account/CIF not listed below falls through unchanged to the existing default
JSON files (`RetailAccountInquiry.json`, `GetCifByAccountNumber.json`, `RetailCIFInquiry.json`,
`AccountFreezeInquiry.json`) — zero effect on any other existing simulator test flow.

## SUCCESS — exact values to type in the browser

| Field | Value |
|---|---|
| Location | **Inside the Country** |
| Account Number | **00115272133** |
| Mobile Number | **9841771361** |
| Email Address | **supriyabudhathoki8@gmail.com** OR **smainali123@gmail.com** — see note below |

> **Note on Email**: `RetailCIFInquiry.json`'s `PhoneEmailInfo` entry with `PhoneOrEmail: "EMAIL"` is
> `SMAINALI123@GMAIL.COM` (comparison is case-insensitive) — use that one; the `supriyabudhathoki8@...`
> address only appears in `RetailAccountInquiry.json`'s `RelPartyContactInfo`, which the application
> never reads for the Mobile/Email match check.

For CIF `02375789`, `RetailAccountInquiry`/`GetCifByAccountNumber`/`RetailCIFInquiry` all now agree
(fixed 2026-08-08 — was previously inconsistent, causing the new CIF-consistency check to reject
before `RetailCIFInquiry` was ever reached).

## Scenario table

| Scenario | Account Number | CIF | Mobile | Email | Expected Application Result |
|---|---|---|---|---|---|
| SUCCESS | 00115272133 | 02375789 | 9841771361 | smainali123@gmail.com | Verified — proceeds to OTP |
| CIF_MISMATCH | 99900000002 | (n/a — RetailAccountInquiry gives 88880002, GetCifByAccountNumber gives 88889902) | any valid | any valid | `Account details mismatch. Please mention correct details.` |
| DORMANT | 99900000003 | 88880003 | any valid | any valid | `Please visit the branch for Account Opening.` |
| FREEZE_STATUS_PRESENT | 99900000004 | 88880004 | any valid | any valid | `Please visit the branch for Account Opening.` |
| DEDICATED_FREEZE | 99900000005 | 88880005 | 9800000005 | testscenario5@example.com | `Please visit the branch for Account Opening.` |
| SUBMIT_FOR_KYC_N | 99900000006 | 88880006 | 9800000006 | testscenario6@example.com | `Please visit the branch for Account Opening.` |
| NON_INDIVIDUAL | 99900000007 | 88880007 | 9800000007 | testscenario7@example.com | `Please visit the branch for Account Opening.` |
| CBS_MOBILE_MISSING | 99900000008 | 88880008 | any valid | any valid | `Please update Contact Details via Mobile App, Video Banking or Branch visit.` |
| CBS_EMAIL_MISSING | 99900000009 | 88880009 | any valid | any valid | `Please update Contact Details via Mobile App, Video Banking or Branch visit.` |
| MOBILE_MISMATCH | 00115272133 | 02375789 | any value ≠ 9841771361 | smainali123@gmail.com | `Account details mismatch. Please mention correct details.` |
| EMAIL_MISMATCH | 00115272133 | 02375789 | 9841771361 | any value ≠ smainali123@gmail.com | `Account details mismatch. Please mention correct details.` |
| ACCOUNT_NOT_FOUND | 99900000012 | — | any valid | any valid | `Account details mismatch. Please mention correct details.` |
| GET_CIF_FAILURE | 99900000013 | (RetailAccountInquiry gives 88880013) | any valid | any valid | `Account details mismatch. Please mention correct details.` |
| RETAIL_CIF_FAILURE | 99900000014 | 88880014 | any valid | any valid | `Account details mismatch. Please mention correct details.` |
| FREEZE_INQUIRY_FAILURE | 99900000015 | 88880015 | 9800000015 | testscenario15@example.com | `Please visit the branch for Account Opening.` |

"any valid" = any 10-digit numeric Mobile / any well-formed Email — the scenario fails before that
field is checked, so its content doesn't affect the outcome.

## FunctionName mappings (unchanged)

`RetailAccountInquiry`, `GetCifByAccountNumber`, `RetailCIFInquiry`, `AccountFreezeInquiry` — literal
values sent/matched exactly as before; nothing in the dispatcher's `FunctionName` comparisons changed.

## Files

- `Controllers/ConnectController.cs` — added scenario lookup (decode `Data`, match account/CIF against
  an optional `<FunctionName>.scenarios.json`), falls back to existing static files unchanged.
- `RetailAccountInquiry.scenarios.json`, `GetCifByAccountNumber.scenarios.json`,
  `RetailCIFInquiry.scenarios.json`, `AccountFreezeInquiry.scenarios.json` — new, scenario data.
- `RetailAccountInquiry.json` — unchanged (already had account 00115272133 / CIF 02375789).
- `GetCifByAccountNumber.json` — fixed to CIF 02375789 / account 00115272133 (was 02730899 /
  00515217484 — the root cause of the reported mismatch), added `ACCT_STATUS`/`FREEZE_STATUS`.
- `RetailCIFInquiry.json` — fixed `CustId` from 02493169 to 02375789 for full consistency (Mobile,
  Email, Status, CustStatus, ConstitutionCode, submitForKYC were already correct — retained as-is).
- `AccountFreezeInquiry.json` — changed from its previous FROZEN sample (`FreezeCode: "T"`) to a clear
  (`OutputRec: []`) response, since this is now the SUCCESS-path default; the original frozen content
  is preserved under the DEDICATED_FREEZE scenario (account 99900000005).
- `api.csproj` — added `CopyToOutputDirectory` entries for the 4 new `.scenarios.json` files.

## Not represented

None — all 15 requested scenarios are covered above.

---

## 2026-08-17 pass — business-accurate mock scenarios (stakeholder samples + derived edge cases)

No architecture changes. Reuses the exact same `ConnectController` scenario dispatch documented above —
`GetCifByAccountNumber` keyed by 11-digit Account Number, `RetailCIFInquiry` keyed by CIF (`CustId`),
both decoded from Base64 `Data`. `ConnectController.cs` was **not modified** in this pass.

### CELLPH confirmation

Confirmed: the correct/actual `PhoneEmailType` value for a preferred mobile record is **`CELLPH`** (no
space) — matches the simulator's own long-standing default `RetailCIFInquiry.json` sample and the
application's own matching code (`ApplyForAccount.cshtml.cs`). All scenarios below use `"CELLPH"`. A
misleading code *comment* (not the executable logic) briefly said `"CELL PH"` — corrected to `"CELLPH"`
this same pass; the executable matching logic was never wrong.

### ✅ SCHM_TYPE is now enforced by the application (fixed 2026-08-17)

`accountopening.web`'s `ApplyForAccountModel.OnPostCheckAccountDetail` now gates on `SCHM_TYPE` from the
`GetCifByAccountNumber` row selected by trimmed `FORACID` match: only **CAA** and **SBA** (case-insensitive)
are allowed to proceed; any other value (`TDA`/`TUA`/`ODA`/unknown/blank) is blocked immediately with
`DiagnosticCode = SCHEME_NOT_ALLOWED` and the same customer-facing branch-visit message used for other
ineligibility cases (`Please visit the branch for Account Opening.`). Account-level `SUBMITFORKYC` is also
now checked (blocks if present and not `Y`, `DiagnosticCode = SUBMIT_KYC_NOT_ALLOWED`) as defense-in-depth
alongside the pre-existing `RetailCIFInquiry`-level `submitForKYC` check. The `INVALID_TDA`/`INVALID_TUA`/
`INVALID_ODA`/`INVALID_UNKNOWN_SCHEME` scenarios below (fixture data unchanged) now correctly resolve to
**BLOCK** / `SCHEME_NOT_ALLOWED` instead of `SUCCESS`.

### Stakeholder sample scenarios (updated 2026-08-17 — latest authoritative sheet)

Account numbers for A, B, and G were corrected to their full 11-digit, leading-zero form per the latest
authoritative source (previously stored as 10-digit values with a confirmation-pending note — that note
is now resolved, not just superseded). Preserved as strings throughout — never parsed/padded/mutated as
numbers.

| Scenario | Customer | Account Number | CIF | SCHM_TYPE | SUBMITFORKYC | ACCT_STATUS | FREEZE_STATUS | Mobile | Email | Expected Result | Reason | Expected Message |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A | CHANDRA KALA GHALE | 02620993040 | 70000001 | SBA | A | DORMANT | (blank) | 9864996127 | (not supplied) | BLOCK | `ACCOUNT_DORMANT` | `Please visit the branch for Account Opening.` |
| B | CHAITRA MANI TIMSINA | 02215137835 | 70000002 | SBA | A | DORMANT | D | 9803455872 | (not supplied) | BLOCK | `ACCOUNT_DORMANT` (primary — dormant is checked before freeze; freeze is also present) | `Please visit the branch for Account Opening.` |
| C | ARJUN KHANAL | 55506709436 | 70000003 | SBA | A | ACTIVE | D | 9767228495 | KHANALARJUN50@GMAIL.COM | BLOCK | `ACCOUNT_FROZEN` | `Please visit the branch for Account Opening.` |
| D | THE INFINITY LOUNGE PRIVATE LIMITED | 55508713567 | 70000004 | CAA | Y (established, not in latest sheet — see SUBMITFORKYC correction note below) | ACTIVE (established, not in latest sheet) | (blank, established) | 9861036238 | CLUBINFINITY61@GMAIL.COM | BLOCK | `NON_INDIVIDUAL_CUSTOMER` | `Please visit the branch for Account Opening.` |
| E | RAJ KUMAR SHRESTHA | 55502681901 | 70000005 | SBA | A | ACTIVE | (blank) | 9843273719 | RAJKUMARSHRESTHA00011@GMAIL.COM | PASS | `ELIGIBLE` | proceeds to OTP |
| F | SANTOSH PUN | 55501785006 | 70000006 | SBA | A | ACTIVE | D | 9763870565 | I.SANTOSHPM@GMAIL.COM | BLOCK | `ACCOUNT_FROZEN` | `Please visit the branch for Account Opening.` |
| G | HIMANSU KUMAR SHAH | 01019534380 | 70000007 | SBA | **A** (real UAT-confirmed — see correction note below) | ACTIVE | (blank) | 9810041180 | HIMANSHUSHAH25@GMAIL.COM | PASS | `ELIGIBLE` | proceeds to OTP |

Row D's Dormant/KYC/Freeze columns are not present in the latest sheet — per instruction, the
already-established simulator values (ACTIVE / no freeze / SUBMITFORKYC=Y) were kept rather than
inventing new ones; the row still blocks on `NON_INDIVIDUAL_CUSTOMER` regardless, since that check
happens at the `RetailCIFInquiry` `ConstitutionCode` stage, after the `GetCifByAccountNumber` fields are
evaluated (and "Y" remains an accepted `SUBMITFORKYC` value — see below — so this doesn't change D's
result even though its true CBS value is unconfirmed).

### ✅ GetCifByAccountNumber.SUBMITFORKYC contract correction (2026-08-17, real UAT evidence)

Real UAT evidence for account `01019534380` (Himansu Kumar Shah, a known expected-success case) returned
`"SUBMITFORKYC": "A"` from `GetCifByAccountNumber` — not `"Y"` as originally assumed when this field was
first added. `ApplyForAccountModel.OnPostCheckAccountDetail`'s account-level check now accepts **both**
`"Y"` and `"A"` (case-insensitive, trimmed); anything else present-but-different still fails closed with
`DiagnosticCode = SUBMIT_KYC_NOT_ALLOWED`, and blank/absent still falls through to the separate
`RetailCIFInquiry`-level `submitForKYC` check (untouched, still requires literal `"Y"` — a different API
field with its own vocabulary, see `EvaluateExistingCustomerEligibility`). `"Y"` was kept rather than
removed since there is no evidence it is actually invalid — only that `"A"` is also valid. Stakeholder
rows A, B, C, E, F, G (all of whose latest-sheet "KYC" column read `A`) had their `GetCifByAccountNumber`
fixture `SUBMITFORKYC` corrected from `"Y"` to `"A"` to match; this changes no row's expected outcome
(A/B still block on dormant first, C/F still block on freeze first, D unchanged/not in scope, E/G still
pass) since both values are accepted. The derived `SUBMIT_KYC_N` scenario (`90000000007`, `SUBMITFORKYC:
"N"`) is intentionally unchanged — still a negative/ineligible case under the corrected rule.

E's and G's exact `RetailCIFInquiry` `PhoneEmailInfo` shapes (`PHONE`/`CELLPH`/`PrefFlag=Y`/
`PhoneNumLocalCode`, `EMAIL`/`PrefFlag=Y`/`EmailInfo`) match the latest sheet and were already correct in
the simulator fixture — no change needed.

**Note**: `supriyabudhathoki8@gmail.com` (referenced in the SUCCESS section above, a separate,
unrelated scenario predating this stakeholder pass) is not part of Himansu Kumar Shah's row and was
never used as his CBS email.

### Derived edge-case scenarios (simulator-only accounts, none represent real customers)

| Scenario ID | Account Number | CIF | SCHM_TYPE | SUBMITFORKYC | ACCT_STATUS | FREEZE_STATUS | Mobile (CBS preferred) | Email (CBS preferred) | Expected Result | Expected Message |
|---|---|---|---|---|---|---|---|---|---|---|
| VALID_CAA | 90000000001 | 80000001 | CAA | Y | ACTIVE | (blank) | 9800000101 | valid.caa@example.com | SUCCESS | proceeds to OTP |
| VALID_SBA | 90000000002 | 80000002 | SBA | Y | ACTIVE | (blank) | 9800000102 | valid.sba@example.com | SUCCESS | proceeds to OTP |
| INVALID_TDA | 90000000003 | 80000003 | TDA | Y | ACTIVE | (blank) | 9800000103 | invalid.tda@example.com | BLOCK (`SCHEME_NOT_ALLOWED`) | `Please visit the branch for Account Opening.` |
| INVALID_TUA | 90000000004 | 80000004 | TUA | Y | ACTIVE | (blank) | 9800000104 | invalid.tua@example.com | BLOCK (`SCHEME_NOT_ALLOWED`) | `Please visit the branch for Account Opening.` |
| INVALID_ODA | 90000000005 | 80000005 | ODA | Y | ACTIVE | (blank) | 9800000105 | invalid.oda@example.com | BLOCK (`SCHEME_NOT_ALLOWED`) | `Please visit the branch for Account Opening.` |
| INVALID_UNKNOWN_SCHEME | 90000000006 | 80000006 | XXX | Y | ACTIVE | (blank) | 9800000106 | invalid.scheme@example.com | BLOCK (`SCHEME_NOT_ALLOWED`) | `Please visit the branch for Account Opening.` |
| SUBMIT_KYC_N | 90000000007 | 80000007 | SBA | N | ACTIVE | (blank) | 9800000107 | submit.kycn@example.com | BLOCK | `Please visit the branch for Account Opening.` |
| DORMANT_ONLY | 90000000008 | 80000008 | SBA | Y | DORMANT | (blank) | 9800000108 | dormant.only@example.com | BLOCK | `Please visit the branch for Account Opening.` |
| FREEZE_ONLY | 90000000009 | 80000009 | SBA | Y | ACTIVE | D | 9800000109 | freeze.only@example.com | BLOCK | `Please visit the branch for Account Opening.` |
| MOBILE_MISMATCH | 90000000010 | 80000010 | SBA | Y | ACTIVE | (blank) | 9800000110 | mobile.mismatch@example.com | BLOCK (type any other mobile) | `Account details mismatch. Please mention correct details.` |
| EMAIL_MISMATCH | 90000000011 | 80000011 | SBA | Y | ACTIVE | (blank) | 9800000111 | email.mismatch@example.com | BLOCK (type any other email) | `Account details mismatch. Please mention correct details.` |
| MOBILE_AND_EMAIL_MISMATCH | 90000000012 | 80000012 | SBA | Y | ACTIVE | (blank) | 9800000112 | both.mismatch@example.com | BLOCK (type other mobile + other email) | `Account details mismatch. Please mention correct details.` |
| MISSING_PREFERRED_MOBILE | 90000000013 | 80000013 | SBA | Y | ACTIVE | (blank) | *(none)* | missing.mobile@example.com | BLOCK | `Please update Contact Details via Mobile app, Video Banking or Branch visit.` |
| MISSING_PREFERRED_EMAIL | 90000000014 | 80000014 | SBA | Y | ACTIVE | (blank) | 9800000114 | *(none)* | BLOCK | `Please update Contact Details via Mobile app, Video Banking or Branch visit.` |
| NON_PREFERRED_PHONE_ONLY | 90000000015 | 80000015 | SBA | Y | ACTIVE | (blank) | 9800000115 (PrefFlag=N, not usable) | nonpref.phone@example.com | BLOCK | `Please update Contact Details via Mobile app, Video Banking or Branch visit.` |
| NON_PREFERRED_EMAIL_ONLY | 90000000016 | 80000016 | SBA | Y | ACTIVE | (blank) | 9800000116 | nonpref.email@example.com (PrefFlag=N, not usable) | BLOCK | `Please update Contact Details via Mobile app, Video Banking or Branch visit.` |
| NON_PREFERRED_BEFORE_PREFERRED_MOBILE | 90000000017 | 80000017 | SBA | Y | ACTIVE | (blank) | array: 9800000117 (N) then **9800000217 (Y)** | order.mobile@example.com | SUCCESS (type 9800000217, the 2nd/preferred entry) | proceeds to OTP |
| NON_PREFERRED_BEFORE_PREFERRED_EMAIL | 90000000018 | 80000018 | SBA | Y | ACTIVE | (blank) | 9800000118 | array: old.nonpreferred@... (N) then **current.preferred@... (Y)** | SUCCESS (type current.preferred@example.com) | proceeds to OTP |
| PHONENUM_VS_LOCALCODE | 90000000019 | 80000019 | SBA | Y | ACTIVE | (blank) | PhoneNum=+9779800000119, PhoneNumLocalCode=**9800000119** | phonenum.test@example.com | SUCCESS (type 9800000119 — proves `PhoneNumLocalCode` is used, not `PhoneNum`) | proceeds to OTP |
| EMAIL_CASE_INSENSITIVE_MATCH | 90000000020 | 80000020 | SBA | Y | ACTIVE | (blank) | 9800000120 | CBS stores `CASE.TEST@EXAMPLE.COM` | SUCCESS (type case.test@example.com, any case) | proceeds to OTP |
| INVALID_NON_INDIVIDUAL | 90000000021 | 80000021 | SBA | Y | ACTIVE | (blank) | 9800000121 | nonindividual@example.com | BLOCK (ConstitutionCode=NONIND) | `Please visit the branch for Account Opening.` |
| GETCIF_FAILURE | 90000000022 | — (GetCifByAccountNumber itself returns not-found) | — | — | — | — | any | any | BLOCK | `Account details mismatch. Please mention correct details.` |
| RETAILCIF_FAILURE | 90000000023 | 80000023 | SBA | Y | ACTIVE | (blank) | any | any | BLOCK | `Account details mismatch. Please mention correct details.` |

### Files changed this pass

- `GetCifByAccountNumber.scenarios.json` — added 7 stakeholder-sample + 23 derived entries; existing 11
  entries untouched.
- `RetailCIFInquiry.scenarios.json` — added 7 stakeholder-sample + 22 derived entries (`GETCIF_FAILURE`
  has no CIF, never reaches this stage); existing 7 entries' `PhoneEmailInfo` records had `PrefFlag: "Y"`
  added (they previously had none — the application's contact-matching logic now requires it on the
  matched record; without this, those 7 pre-existing scenarios would have silently stopped resolving a
  mobile/email and always returned the contact-update message instead of their originally intended
  outcome).
- `ConnectController.cs` — **not modified**. Existing generic scenario dispatch already supports every
  case above.
- `accountopening.web/.../ApplyForAccount.cshtml.cs` — comment-only correction (misleading `"CELL PH"`
  text in a code comment corrected to `"CELLPH"`, matching the executable logic, which was already
  correct). No logic change.

## 2026-10-15 — End-to-end UAT dataset (TC01–TC06) and created-CIF persistence

| TC | Purpose | Account | CIF | Mobile / Email | Notes |
|---|---|---|---|---|---|
| TC01 | New individual, no CIF | n/a | n/a | any | Use the portal "new customer" path; no CBS lookup. After Branch creates the CIF, simulator issues `31000001…` and keeps it fetchable. |
| TC02 | Existing individual, editable mobile/email | 55509991002 | 19991002 | 9800000202 / tc02.existing@example.com | Eligible (SBA, ACTIVE, SUBMITFORKYC=Y). Change mobile/email, then verify RetailCIFInquiry reflects RetCustModKYC. |
| TC03 | Minor with existing Guardian CIF | 55509991003 (Guardian) | 19991003 | 9800000203 / tc03.guardian@example.com | Search Guardian by this account; applicant DOB < 18. |
| TC04 | Minor, Guardian without CIF | n/a | n/a | any | Enter Guardian manually as new; Guardian CIF is generated like TC01. |
| TC05 | Existing CIF, incomplete optional data | 55509991005 | 19991005 | 9800000205 / none | Occupation, PAN, Education, place of birth, e-mail all empty. |
| TC06 | Existing PEP=Yes, HIGH AML | 55509991006 | 19991006 | 9800000206 / tc06.pep@example.com | `GetKycDetailByCif` -> PEPCONFIRMATION=YES, AML_RISK_RATING=HRISK. |

Behaviour added: `GetKycDetailByCif.scenarios.json` (per-CIF, falls back to GetKycDetailByCIF.json); `RetailCifOpening` / `RetailAccountOpening` now return unique sequential CIF (31000001…) / account (55531000001…) numbers and remember them in memory so `RetailCIFInquiry`, `GetCifByAccountNumber`, `AccountFreezeInquiry` find them (reset on restart); startup refuses `ASPNETCORE_ENVIRONMENT=Production` unless `Simulator:AllowProduction=true`. Created CIFs reuse the default inquiry template for non-identity fields (PAN, phone, e-mail are placeholder values).
Not changed: NID verification (always HIT; `SimulateNoHit` const), ENADOC token/documents (static).
