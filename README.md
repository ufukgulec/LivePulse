# ⚡ LivePulse - Gerçek Zamanlı Dağıtık Telemetri Paneli

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Core-green?logo=nuget)](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
[![Redis](https://img.shields.io/badge/Redis-Backplane-DC382D?logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://www.docker.com/)

**LivePulse**; **.NET 10**, **ASP.NET Core SignalR**, **Redis Scale-Out Backplane** ve **Docker** teknolojileriyle geliştirilmiş, hafif ve yüksek performanslı bir gerçek zamanlı telemetri izleme uygulamasıdır.

Proje, birden fazla sunucu düğümünde yatay ölçeklenebilen ve gerçek zamanlı mesajları farklı sunucu örnekleri arasında dağıtabilen kurumsal sistemlerin mimarisini uygulamalı olarak gösterir.

Temel amaç; CPU ve RAM kullanım metrikleri gibi telemetri verilerinin birden fazla API sunucusu üzerinden üretilmesini ve Redis aracılığıyla tüm bağlı istemcilere gerçek zamanlı olarak iletilmesini sağlamaktır.

---

## 📸 Demo ve Ekran Görüntüleri

<img width="1785" height="830" alt="Ekran Görüntüsü 2026-10-10 17-24-22" src="https://github.com/user-attachments/assets/bdc920fa-bdaa-4d04-a346-79059a2c7aa1" />

> **Ekran görüntüsünün gösterdiği:** `:5000` portunda çalışan `Node-Alpha` ve `:5001` portunda çalışan `Node-Beta` sunucularının CPU/RAM telemetri verilerini Redis Backplane üzerinden senkronize biçimde yayınlaması.

---

## 🏗️ Mimari Genel Bakış

```text
                         ┌───────────────────────────┐
                         │     Tarayıcı İstemcileri  │
                         └─────────────┬─────────────┘
                                       │
                   ┌───────────────────┴───────────────────┐
                   │                                       │
          (WebSocket Bağlantısı)                  (WebSocket Bağlantısı)
                   │                                       │
                   ▼                                       ▼
     ┌──────────────────────────┐             ┌──────────────────────────┐
     │   Node-Alpha (API 1)     │             │   Node-Beta (API 2)      │
     │   Port: 5000             │             │   Port: 5001             │
     │   Background Worker      │             │   Background Worker      │
     └─────────────┬────────────┘             └─────────────┬────────────┘
                   │                                       │
                   └───────────────────┬───────────────────┘
                                       │
                                       ▼
                         ┌───────────────────────────┐
                         │      Redis Backplane      │
                         │    (Mesaj Dağıtıcısı)     │
                         └───────────────────────────┘
```

### 🔄 Yatay Ölçeklendirme Nasıl Çalışır?

Geleneksel bir WebSocket mimarisinde, `Node-Alpha` sunucusuna bağlı bir istemci, yalnızca `Node-Beta` üzerinde üretilen mesajları otomatik olarak alamaz. Her sunucu kendi bağlantılarını yönetir.

LivePulse, bu sorunu **SignalR Redis Backplane** kullanarak çözer.

İşleyiş şu şekildedir:

1. API sunucularındaki arka plan servisleri telemetri verilerini üretir.
2. Veriler, strongly typed SignalR Hub arayüzleri üzerinden istemcilere gönderilir.
3. Redis Backplane, SignalR mesajlarının sunucu örnekleri arasında dağıtılmasını sağlar.
4. Her sunucu, kendi üzerinde bağlı olan istemcilere ilgili mesajları iletir.
5. Böylece farklı API sunucularına bağlı istemciler aynı gerçek zamanlı yayınları alabilir.

Bu yaklaşım, uygulamanın tek bir sunucu örneğine bağlı kalmadan yatay olarak ölçeklendirilmesine yardımcı olur.

> **Önemli:** Redis Backplane, SignalR mesajlarının sunucu örnekleri arasında dağıtılmasını sağlar. İstemci bağlantılarının kopması, mesajların kalıcı olarak saklanması veya kesinti sırasında kaybolan mesajların yeniden oynatılması gibi ihtiyaçlar ayrıca ele alınmalıdır.

---

## 🚀 Öne Çıkan Teknik Özellikler

### 1. Strongly Typed SignalR Hub

SignalR istemci iletişiminde metin tabanlı metot isimleri yerine C# arayüzleri kullanılır.

Örneğin:

```csharp
IHubContext<MetricsHub, IMetricsClient>
```

Bu yaklaşımın avantajları:

- Derleme zamanında tip güvenliği sağlar.
- Metot isimlerinden kaynaklanan hataları azaltır.
- İstemci-sunucu iletişimini daha yönetilebilir hâle getirir.
- Refactoring işlemlerini kolaylaştırır.

### 2. Bağımsız Arka Plan Servisleri

`MetricsPublisherWorker`, `BackgroundService` veya `IHostedService` altyapısını kullanarak telemetri verilerini sürekli üretir.

Bu sayede:

- Telemetri üretimi HTTP isteklerinden bağımsız çalışır.
- Periyodik görevler arka plan servisleri üzerinden yürütülür.
- Gerçek zamanlı veri akışı uygulama yaşam döngüsüyle entegre edilir.

### 3. Redis ile Çoklu Sunucu Desteği

Proje, `api1` ve `api2` gibi birden fazla API örneğinde çalışacak şekilde tasarlanmıştır.

Redis Backplane sayesinde:

- Farklı sunuculardaki SignalR Hub mesajları dağıtılabilir.
- İstemciler farklı API örneklerine bağlansalar bile ortak yayınları alabilir.
- Uygulama yatay ölçeklendirme senaryolarına uygun hâle gelir.

### 4. Dayanıklı İstemci Bağlantısı

Ön yüz, Vanilla JavaScript ve Chart.js ile geliştirilmiştir.

SignalR istemcisi, `withAutomaticReconnect()` özelliğini kullanarak belirli bağlantı kopmalarından sonra yeniden bağlanmayı dener.

Bu yaklaşım, geçici bağlantı sorunlarının kullanıcı deneyimine etkisini azaltır.

> `withAutomaticReconnect()` ilk bağlantı kurulumunun başarısız olması durumunda otomatik olarak yeniden denemeyi garanti etmez. İlk bağlantı ve yeniden bağlanma davranışları ayrı ele alınmalıdır.

### 5. Container Tabanlı Çalıştırma

Uygulama Docker ve Docker Compose ile container ortamında çalıştırılabilir.

- Çok aşamalı Docker build yaklaşımı
- Redis ve birden fazla API örneğinin birlikte çalıştırılması
- Tekrarlanabilir yerel geliştirme ortamı
- Servislerin merkezi olarak yönetilmesi

Bu yapı, yerel ortamda gerekli servisleri tek bir Compose yapılandırmasıyla başlatmayı kolaylaştırır.

---

## 🛠️ Teknoloji Yığını

| Teknoloji | Kullanım Amacı |
|---|---|
| **.NET 10** | Uygulama altyapısı |
| **ASP.NET Core Web API** | API sunucuları |
| **ASP.NET Core SignalR** | Gerçek zamanlı iletişim |
| **StackExchange.Redis** | Redis bağlantısı ve SignalR Backplane entegrasyonu |
| **Microsoft.AspNetCore.SignalR.StackExchangeRedis** | SignalR için Redis ölçeklendirme desteği |
| **BackgroundService** | Arka plan telemetri üretimi |
| **JavaScript** | İstemci tarafı etkileşimleri |
| **HTML5 / CSS** | Dashboard arayüzü |
| **Chart.js** | Telemetri grafiklerinin görselleştirilmesi |
| **Docker** | Container oluşturma ve çalıştırma |
| **Docker Compose** | Servislerin birlikte yönetilmesi |

---

## 📂 Proje Yapısı

```text
LivePulse/
├── src/
│   └── LivePulse.Api/
│       ├── Hubs/
│       │   ├── IMetricsClient.cs
│       │   └── MetricsHub.cs
│       │
│       ├── Services/
│       │   └── MetricsPublisherWorker.cs
│       │
│       ├── wwwroot/
│       │   └── index.html
│       │
│       ├── Program.cs
│       ├── Dockerfile
│       └── LivePulse.Api.csproj
│
├── docs/
│   └── screenshot.png
│
├── docker-compose.yml
├── LICENSE
└── README.md
```

### Temel Dosyalar

| Dosya | Açıklama |
|---|---|
| `IMetricsClient.cs` | Strongly typed SignalR istemci sözleşmesini tanımlar. |
| `MetricsHub.cs` | SignalR Hub uygulamasını içerir. |
| `MetricsPublisherWorker.cs` | Arka planda telemetri verisi üretir ve yayınlar. |
| `index.html` | Gerçek zamanlı dashboard arayüzünü içerir. |
| `Program.cs` | Uygulama yapılandırmasını, servis kayıtlarını ve Redis entegrasyonunu yönetir. |
| `Dockerfile` | API uygulamasının container imajını oluşturur. |
| `docker-compose.yml` | Redis ve iki API örneğini birlikte çalıştırır. |

---

## ⚡ Hızlı Başlangıç

### Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Git
- Docker Compose desteği

### 1. Repository'yi Klonlayın

Aşağıdaki örnekte repository URL'sini kendi GitHub adresinizle değiştirin:

```bash
git clone https://github.com/your-username/LivePulse.git
cd LivePulse
```

### 2. Servisleri Başlatın

Docker Compose ile Redis ve API örneklerini oluşturup arka planda çalıştırın:

```bash
docker compose up --build -d
```

Container durumlarını kontrol edin:

```bash
docker compose ps
```

Servis loglarını takip edin:

```bash
docker compose logs -f
```

### 3. Dashboard'ları Açın

Tarayıcınızda aşağıdaki adresleri açın:

- **Node-Alpha Dashboard:** [http://localhost:5000](http://localhost:5000)
- **Node-Beta Dashboard:** [http://localhost:5001](http://localhost:5001)

### 4. Gerçek Zamanlı Senkronizasyonu Doğrulayın

İki dashboard'u yan yana açın.

Her iki API örneği de kendi telemetri verilerini üretebilir. Redis Backplane üzerinden dağıtılan SignalR mesajlarının, farklı sunuculara bağlı istemcilerde de görüntülendiğini gözlemleyin.

Bu test, birden fazla sunucu örneğinde gerçek zamanlı mesaj dağıtımının nasıl çalıştığını anlamaya yardımcı olur.

---

## 🧹 Ortamı Durdurma

Container'ları durdurup Compose tarafından oluşturulan container ve ağ kaynaklarını kaldırmak için:

```bash
docker compose down
```

Redis verilerinin bir volume üzerinde saklandığı bir yapılandırma kullanıyorsanız, `docker compose down` varsayılan olarak bu volume'ü silmez.

Volume'leri de kaldırmak için:

```bash
docker compose down -v
```

> **Dikkat:** `-v` parametresi kalıcı volume verilerini silebilir. Bu komutu yalnızca ilgili verileri silmek istediğinizden eminseniz kullanın.

---


## 🎯 Projenin Amacı

LivePulse, aşağıdaki konularda uygulamalı deneyim kazanmak amacıyla geliştirilmiştir:

- .NET ile gerçek zamanlı uygulama geliştirme
- SignalR Hub ve strongly typed istemci iletişimi
- Redis Backplane ile dağıtık mesaj iletimi
- Background Service mimarisi
- Birden fazla API örneğiyle yatay ölçeklendirme
- Docker ve Docker Compose ile container tabanlı geliştirme
- JavaScript ve Chart.js ile gerçek zamanlı veri görselleştirme
