# طريقة التثبيت والبناء

التحذير `dotnet: command not found` يعني أن جهاز البناء لا يحتوي على .NET SDK. هذا ليس خطأ في الكود؛ يجب تثبيت .NET 10 SDK أولاً ثم بناء المشروع عبر SharpProspero.

## 1. ثبّت .NET 10 SDK على جهاز الكمبيوتر

### Windows

```powershell
winget install Microsoft.DotNet.SDK.10
```

ثم أغلق وافتح PowerShell وتأكد:

```powershell
dotnet --info
```

### Ubuntu / Debian Linux

استخدم تعليمات Microsoft الرسمية المناسبة لإصدار التوزيعة لديك، ثم تأكد:

```bash
dotnet --info
```

### macOS

نزّل مثبت .NET 10 SDK الرسمي من Microsoft، ثم تأكد:

```bash
dotnet --info
```

## 2. جهّز SharpProspero

ضع مجلد SharpProspero SDK على جهازك، ثم عرّف المتغير `SHARPPROSPERO_ROOT` ليشير إليه.

### Windows PowerShell

```powershell
setx SHARPPROSPERO_ROOT "C:\path\to\SharpProspero"
```

بعد `setx` أغلق وافتح PowerShell من جديد.

### Linux / macOS

```bash
export SHARPPROSPERO_ROOT="/path/to/SharpProspero"
```

## 3. ابنِ المشروع

من مجلد هذا المشروع شغّل:

```powershell
pwsh ./build.ps1
```

إذا أردت إخراج الملفات في مجلد بدلاً من payload فقط:

```powershell
pwsh ./build.ps1 -Output Folder
```

## 4. التشغيل على الجهاز

بعد نجاح البناء، خذ ملف الـ payload ELF الناتج من SharpProspero وشغّله على الجهاز بالطريقة التي تستخدمها لتشغيل payloads.

بعد التشغيل افتح من المتصفح:

```text
http://<console-ip>:8080
```

- اضغط **Lock CUSA Folders** قبل Rebuild Database.
- اضغط **Unlock CUSA Folders** قبل تثبيت أو تحديث fpkg.

## ملاحظات مهمة

- لا تستخدم ملف ELF قديم مبني بـ `cc` على Linux؛ هذا المشروع الآن يعتمد على SharpProspero لإنتاج ELF مناسب.
- البرنامج يغيّر صلاحيات مجلدات `CUSA*` فقط داخل `/user/app`, `/user/addcont`, `/user/patch`, و`/user/playgo`.
- ملفات الحفظ ليست ضمن المسارات المستهدفة.
