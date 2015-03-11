Mozu Webtoolkit
============

<b>Version 1.2.3</b><br>

1) Update ConfigurationAuthFilter to validate tenant Access<br>
2) Update ApiAuthFilter to validate requests for the tenantId.To validate the requests pass these two headers x-vol-tenant & x-vol-hmac-sha256 in addition to form & cookie token. hmac can be generated using Mozu.Api.Security.SHA256Generator.GetHash(string.Empty,tenantId.ToString())
