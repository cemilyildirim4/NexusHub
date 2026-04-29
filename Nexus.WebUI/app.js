// app.js

// 1. Backend URL'ini buraya yapıştır (Swagger'dan kontrol et!)
const API_URL = "https://localhost:7230/api/device";

// 2. Sidebar Mantığı
const btn = document.querySelector("#btn");
const sidebar = document.querySelector(".sidebar");

btn.onclick = function () {
  sidebar.classList.toggle("open");
};

// 3. Sayfa Yönetimi
document.getElementById("nav-dashboard").onclick = () => renderDashboard();
document.getElementById("nav-devices").onclick = () => fetchAndRenderDevices();

// Dashboard Sayfası
function renderDashboard() {
  document.getElementById("page-title").innerText = "Genel Bakış";
  document.getElementById("main-content").innerHTML = `
        <div class="nexus-card">
            <h2>NexusHub'a Hoş Geldiniz</h2>
            <p style="color:#707070; margin-top:10px;">
                Cihazlarınızın anlık durumunu izlemek, yeni cihaz eklemek veya yönetmek için sol menüyü kullanabilirsiniz.
            </p>
        </div>`;
}
async function addDevice() {
  const nameInput = document.getElementById("devName");
  const ipInput = document.getElementById("devIp");

  if (!nameInput.value || !ipInput.value) {
    alert("Lütfen alanları doldurun!");
    return;
  }

  const newDevice = {
    Name: nameInput.value,
    IpAddress: ipInput.value,
    Status: "Checking...", 
    LastSeen: new Date().toISOString(),
  };

  try {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(newDevice),
    });

    if (response.ok) {
      const result = await response.json();
      console.log("Backend'den dönen cevap:", result);

      // Kutuları temizle
      nameInput.value = "";
      ipInput.value = "";

      // Listeyi tazele
      fetchAndRenderDevices();
    }
  } catch (error) {
    console.error("Ekleme sırasında hata:", error);
  }
}

async function deleteDevice(id) {
    if(!confirm("Bu cihazı silmek istediğinize emin misiniz?")) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            fetchAndRenderDevices(); // Listeyi güncelle
        } else {
            alert("Silme işlemi başarısız oldu.");
        }
    } catch (error) {
        console.error("Silme hatası:", error);
    }
}


// Cihaz Listesi Sayfası (API'den veri çeker)
async function fetchAndRenderDevices() {
  document.getElementById("page-title").innerText = "Cihaz Listesi";
  const container = document.getElementById("main-content");

  // Yükleniyor animasyonu/mesajı
  container.innerHTML = `<div class="nexus-card"><p>Veriler yükleniyor...</p></div>`;

  try {
    // Backend'e istek atıyoruz
    const response = await fetch(API_URL);

    if (!response.ok) {
      throw new Error(`API Hatası: ${response.status}`);
    }

    const devices = await response.json();

    // Veri yoksa mesaj göster
    if (devices.length === 0) {
      container.innerHTML = `<div class="nexus-card"><p>Sistemde kayıtlı cihaz bulunamadı.</p></div>`;
      return;
    }

    // Tablo HTML'ini oluştur
    let tableHtml = `
            <div class="nexus-card">
                <table class="styled-table">
                    <thead>
                        <tr>
                            <th>Cihaz Adı</th>
                            <th>IP Adresi</th>
                            <th>Durum</th>
                            <th>Son Görülme</th>
                        </tr>
                    </thead>
                    <tbody>`;

    devices.forEach((d) => {
      // Duruma göre stil belirle (Healthy -> Green, Critical -> Red)
      const statusClass =
        d.status === "Healthy" ? "status-healthy" : "status-critical";

      // ... tablo döngüsünün içindeki satır yapısı ...
      tableHtml += `
    <tr>
        <td><strong>${d.name}</strong></td>
        <td>${d.ipAddress}</td>
        <td><span class="status-badge ${statusClass}">${d.status}</span></td>
        <td>${new Date(d.lastSeen).toLocaleString("tr-TR")}</td>
        <td>
            <button onclick="deleteDevice(${d.id})" class="delete-btn">
                <i class='bx bx-trash'></i>
            </button>
        </td>
    </tr>`;
    });

    tableHtml += `</tbody></table></div>`;
    container.innerHTML = tableHtml;
  } catch (error) {
    console.error("Hata Detayı:", error);
    container.innerHTML = `
            <div class="nexus-card" style="border:1px solid #f5c6cb; background:#f8d7da; color:#721c24;">
                <h4>Bağlantı Hatası!</h4>
                <p>Backend (API) projesine bağlanılamadı. Lütfen şunları kontrol edin:</p>
                <ul style="margin-left:20px; margin-top:10px;">
                    <li>Visual Studio'da Backend projesi çalışıyor mu?</li>
                    <li>app.js içindeki API_URL portu Swagger ile aynı mı?</li>
                    <li>Backend'de Program.cs'te CORS ayarları yapıldı mı?</li>
                </ul>
            </div>`;
  }
}

// Sayfa ilk açıldığında dashboard'u göster
renderDashboard();
