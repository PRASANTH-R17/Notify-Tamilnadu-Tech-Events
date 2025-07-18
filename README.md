# Notifier TamilNadu Tech Events

This project monitors Tamilnadu.Tech's RSS feed and community event sources (via JSON) and sends timely email notifications about new events straight to your inbox.

With this tool, you can:

* ✅ Subscribe to get event notifications via email
* ❌ Unsubscribe anytime to stop receiving notifications
* 📥 Get updates as soon as new events are published
* 🔄 Automatically checks for updates every 30 minutes

---

## 📧 Sample Email Notification
![Sample Email](https://github.com/user-attachments/assets/0c9482c6-017c-49c9-ab9b-be83180658f8)



## Features

* **GitHub Events Integration:** Fetches latest events from GitHub JSON feed
* **Mail Notification:** Sends detailed HTML email updates to subscribers
* **Google Sheets Integration:** Uses Google Sheets for managing subscriber data
* **Auto Update Check:** Checks for new commits and event updates every 30 minutes

---

## How to Use

1. Clone this repository
2. Configure your **appsettings.json** with your email and sheet credentials
3. Add your subscribers via the integrated form or Google Sheet
4. Run the project — it will start tracking and sending notifications

---

## Subscription Options

* **Subscribe:** Receive event updates in your inbox
* **Unsubscribe:** Stop receiving notifications anytime

---

## Tech Stack

* C# .NET
* GitHub API
* Google Sheets API
* SMTP Mail Service

---

## Contributing

Feel free to fork, submit pull requests, or raise issues! Contributions to improve functionality or add new features are welcome.

---

## Disclaimer

This is a community-driven Proof of Concept (PoC) for learning and community engagement. It is not affiliated with or endorsed by Tamilnadu.tech. The tool fetches publicly available data respecting Tamilnadu.tech’s usage policies.
---