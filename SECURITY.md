# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 2.x     | Yes       |
| 1.x     | No        |

## Reporting a Vulnerability

If you discover a security vulnerability in this project, please **do not** open a public GitHub issue.

Instead, report it privately by emailing the repository owner via the contact on their GitHub profile, or by using [GitHubs private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing/privately-reporting-a-security-vulnerability).

Please include:
- A description of the vulnerability
- Steps to reproduce
- Potential impact
- Whether local SQLite data, receipt output, logs, or payment-reference handling are affected

You will receive a response within 72 hours. We will work with you to confirm the issue and release a patch before any public disclosure.

## Security Notes

- This project is local-first and stores order data in SQLite under `%APPDATA%\PizzaExpress`.
- Non-cash checkout uses a reference field only; contributors should not add full card-number storage.
- Crash logs should remain useful for debugging without leaking unnecessary customer or payment data.
