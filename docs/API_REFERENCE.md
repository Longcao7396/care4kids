# API Reference

> Auto-generated summary of REST endpoints exposed by the ASP.NET Web API.
> For each endpoint: HTTP method, path, auth requirement, and purpose.
> Always verify against the controller source — this file is a quick lookup, not the source of truth.

---

## `Achievements`  (prefix: `api/achievements`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/achievements` → `GetAll(bool activeOnly = true,
            string category = null,
            int page = 1,
            int pageSize = 20)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/achievements/stats` → `GetStats()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/achievements/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/achievements` → `Create(Achievement item)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/achievements/{id:int}` → `Update(int id, Achievement item)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/achievements/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `AdminDashboard`  (prefix: `api/admin`)

- **HttpGet** `api/admin/stats` → `GetDashboardStats()` _[public]_
- **HttpGet** `api/admin/recent-donations` → `GetRecentDonations(int count = 20)` _[public]_
- **HttpGet** `api/admin/users-stats` → `GetUsersStats()` _[public]_
- **HttpGet** `api/admin/donations` → `GetAllDonations(int page = 1,
            int pageSize = 20,
            string status = null,
            string search = null,
            int? campaignId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)` _[public]_

## `AdminUsers`  (prefix: `api/admin/users`)

- **HttpGet** `api/admin/users` → `GetAll(int page = 1,
            int pageSize = 20,
            string role = null,
            string search = null)` _[public]_
- **HttpPut** `api/admin/users/{id:int}/role` → `UpdateRole(int id, [FromBody] RoleUpdateDto payload)` _[public]_
- **HttpPut** `api/admin/users/{id:int}/status` → `UpdateStatus(int id, [FromBody] StatusUpdateDto payload)` _[public]_
- **HttpDelete** `api/admin/users/{id:int}` → `Delete(int id)` _[public]_

## `AuthBootstrap`  (prefix: `api/auth`)

- **HttpPost** `api/auth/bootstrap` → `Bootstrap()` _[public]_

## `Auth`  (prefix: `api/auth`)

- **HttpPost** `api/auth/login` → `Login(LoginRequest request)` _[public]_
- **HttpGet** `api/auth/me` → `Me()` _[public]_
- **HttpPost** `api/auth/register` → `Register([FromBody] RegisterViewModel request)` _[public]_
- **HttpPost** `api/auth/logout` → `Logout()` _[public]_

## `Campaigns`  (prefix: `api/campaigns`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/campaigns` → `GetAll(string status = null,
            int? causeId = null,
            bool? featured = null,
            string search = null,
            // Optional: filter to events (RegistrationRequired = true)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/campaigns/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/campaigns` → `Create(CampaignCreateRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/campaigns/{id:int}` → `Update(int id, CampaignUpdateRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/campaigns/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/campaigns/featured` → `GetFeatured(int count = 3)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/campaigns/{id:int}/register` → `Register(int id, CampaignRegistrationRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/campaigns/my-registrations` → `GetMyRegistrations()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/campaigns/{id:int}/registrations` → `GetRegistrations(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Careers`  (prefix: `api/careers`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/careers` → `GetAll(bool activeOnly = true, string department = null, string employmentType = null)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/careers/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/careers/{id:int}/apply` → `Apply(int id, CareerApplyRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/careers/{id:int}/applications` → `GetApplications(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/careers` → `Create(Career career)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/careers/{id:int}` → `Update(int id, Career career)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/careers/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Causes`  (prefix: `api/causes`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/causes` → `GetAll(bool activeOnly = true, bool parentsOnly = false, int? subOf = null)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/causes/tree` → `GetTree(bool activeOnly = true)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/causes/{id:int}/sub-causes` → `GetSubCauses(int id, bool activeOnly = true)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/causes/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/causes/stats` → `GetStats()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/causes` → `Create(Cause cause)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/causes/{id:int}` → `Update(int id, Cause cause)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/causes/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `CmsPages`  (prefix: `api/cms`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin,ContentManager")]`)

- **HttpGet** `api/cms/pages` → `GetPages(string keys = null, bool includeInactive = false)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_
- **HttpGet** `api/cms/pages/{key}` → `GetByKey(string key)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_
- **HttpGet** `api/cms/pages/id/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_
- **HttpPost** `api/cms/pages` → `Create(CmsPageCreateRequest request)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_
- **HttpPut** `api/cms/pages/{id:int}` → `Update(int id, CmsPageUpdateRequest request)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_
- **HttpDelete** `api/cms/pages/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin,ContentManager")]_

## `Contacts`  (prefix: `api/contacts`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpPost** `api/contacts` → `Submit(ContactSubmitRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/contacts` → `GetAll(bool? isRead = null,
            string search = null,
            int page = 1,
            int pageSize = 20)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/contacts/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/contacts/{id:int}/reply` → `Reply(int id, ContactReplyRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/contacts/{id:int}/read` → `ToggleRead(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/contacts/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/contacts/stats` → `GetStats()` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Conversations`  (prefix: `api/conversations`)

- **HttpPost** `api/conversations` → `Create(ConversationCreateRequest request)` _[public]_
- **HttpPost** `api/conversations/{id:int}/messages` → `AddMessage(int id, MessageRequest request)` _[public]_
- **HttpPost** `api/conversations/{id:int}/close` → `Close(int id)` _[public]_
- **HttpGet** `api/conversations/mine` → `GetMine(string status = null)` _[public]_
- **HttpGet** `api/conversations/{id:int}` → `GetById(int id)` _[public]_
- **HttpGet** `api/conversations` → `GetAll(string status = null, string priority = null,
            string type = null, int page = 1, int pageSize = 20)` _[public]_
- **HttpGet** `api/conversations/stats` → `GetStats()` _[public]_
- **HttpPost** `api/conversations/{id:int}/assign` → `Assign(int id, AssignRequest request)` _[public]_

## `Donations`  (prefix: `api/donations`)

- **HttpGet** `api/donations` → `GetAll(int page = 1, int pageSize = 10)` _[JwtAuth]_
- **HttpGet** `api/donations/{id:int}` → `GetById(int id)` _[public]_
- **HttpPost** `api/donations` → `Create(DonationRequest request)` _[public]_
- **HttpGet** `api/donations/stats` → `GetStats()` _[public]_

## `Faqs`  (prefix: `api/faqs`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/faqs` → `GetAll(bool activeOnly = true, string category = null, string search = null)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/faqs/categories` → `GetCategories()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/faqs/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/faqs` → `Create(Faq faq)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/faqs/{id:int}` → `Update(int id, Faq faq)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/faqs/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Gallery`  (prefix: `api/gallery`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/gallery` → `GetAll(bool featuredOnly = false,
            string category = null,
            int? programmeId = null,
            int page = 1,
            int pageSize = 24)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/gallery/categories` → `GetCategories()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/gallery/programmes` → `GetProgrammes()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/gallery/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/gallery` → `Create(GalleryRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/gallery/{id:int}` → `Update(int id, GalleryRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/gallery/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Health`  (prefix: `(no prefix)`)

- **HttpGet** `(no prefix)` → `Index()` _[public]_
- **HttpGet** `(no prefix)/health` → `Health()` _[public]_

## `Invitations`  (prefix: `api/invitations`)

- **HttpPost** `api/invitations` → `Send(InvitationRequest request)` _[public]_
- **HttpGet** `api/invitations/mine` → `GetMine()` _[public]_
- **HttpGet** `api/invitations/stats` → `GetStats()` _[public]_
- **HttpGet** `api/invitations` → `GetAll(string status = null, int page = 1, int pageSize = 30)` _[public]_
- **HttpPost** `api/invitations/{id:int}/cancel` → `Cancel(int id)` _[public]_
- **HttpPost** `api/invitations/accept/{token}` → `AcceptByToken(string token)` _[public]_

## `Organizations`  (prefix: `api/organizations`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/organizations` → `GetAll(bool activeOnly = false,
            string type = null,
            int page = 1,
            int pageSize = 20)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/organizations/stats` → `GetStats()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/organizations/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/organizations` → `Create(OrganizationRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/organizations/{id:int}` → `Update(int id, OrganizationRequest request)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/organizations/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Programmes`  (prefix: `api/programmes`)

- **HttpGet** `api/programmes` → `GetAll(string programmeType = null, string status = null, int page = 1, int pageSize = 10, bool featuredOnly = false)` _[public]_
- **HttpGet** `api/programmes/{id:int}` → `GetById(int id)` _[public]_
- **HttpPost** `api/programmes/{id:int}/register` → `Register(int id, ProgrammeRegistrationRequest request)` _[public]_
- **HttpGet** `api/programmes/my-registrations` → `GetMyRegistrations()` _[public]_
- **HttpGet** `api/programmes/{id:int}/registrations` → `GetRegistrations(int id)` _[public]_
- **HttpPost** `api/programmes` → `Create(Programme programme)` _[public]_
- **HttpPut** `api/programmes/{id:int}` → `Update(int id, Programme programme)` _[public]_

## `Supporters`  (prefix: `api/supporters`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/supporters` → `GetAll(bool activeOnly = true, string type = null)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/supporters/stats` → `GetStats()` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/supporters/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/supporters` → `Create(Organization organization)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/supporters/{id:int}` → `Update(int id, Organization organization)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/supporters/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Team`  (prefix: `api/team`, class auth: `[JwtAuthorize(Roles = "SuperAdmin,Admin")]`)

- **HttpGet** `api/team` → `GetAll(bool activeOnly = true,
            bool featuredOnly = false,
            int page = 1,
            int pageSize = 20)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpGet** `api/team/{id:int}` → `GetById(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPost** `api/team` → `Create(TeamMember member)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpPut** `api/team/{id:int}` → `Update(int id, TeamMember member)` _[class-level (Roles = "SuperAdmin,Admin")]_
- **HttpDelete** `api/team/{id:int}` → `Delete(int id)` _[class-level (Roles = "SuperAdmin,Admin")]_

## `Users`  (prefix: `api/users`)

- **HttpGet** `api/users/me` → `GetMe()` _[public]_
- **HttpPut** `api/users/me` → `UpdateMe(UpdateProfileRequest request)` _[public]_


---

_Last auto-generated: 2026-09-15_
