# Security policy

## Supported versions

Only the latest major version of `Microsoft.Extensions.Logging.Log4Net.AspNetCore` receives security fixes. Upgrade to the latest release before reporting an issue.

| Version | Supported |
| --- | --- |
| 10.x | Yes |
| Earlier than 10.0 | No |

The sample applications under `samples/` are not supported and are not shipped in the package. See [Security status of sample applications](README.md#security-status-of-sample-applications) for their known dependency findings.

## Reporting a vulnerability

Do not open a public issue for a security vulnerability.

Report it privately through [GitHub private vulnerability reporting](https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/security/advisories/new). Include the affected package version, the target framework, a minimal reproduction, and the impact you observed.

You can expect an acknowledgement within 7 days. Confirmed issues are fixed in a new release and disclosed through a GitHub security advisory once the fix is available.

## Scope

This package forwards messages, exceptions and scope values to log4net without encoding them. Sanitise untrusted input, such as carriage returns and line feeds from user-supplied values, before logging it, or use a log4net layout that encodes it.

Vulnerabilities in log4net itself belong to the [Apache log4net project](https://logging.apache.org/security.html).
