# Projeler

- HelloWorld
  - Bu projenin içerisinde RabbitMQ tarafında yer alan publisher ve subscriber bulunmaktadır. Publisher tarafında farklı exchange türlerine göre kuyruk oluşturduğu, subscriber tarafında ise bu kuyrukların nasıl dinlendiğinin kodları yazmaktadır.
- AddWatermark
  - Bu proje, ASP.NET Core MVC ile yazılmış olup amacı, yüklenilen resme watermark ekleme işlemini yaparken kullanıcıyı bekletmeden bu işlemi yapmasıdır.
  - Aynı proje içerisinde publisher ve consumer (subscriber) oluşturuldu.
  - Arkaplan servisi ile kuyruğa gönderilen event in nasıl dinlendiği ve consume edildiği öğrenildi.
- CreateExcel
  - Bu proje, varolan bir veritabanına ait tablonun verilerini Excel tablosuna kaydetmek amacıyla oluşturulmuş ASP.NET Core MVC projesidir.
  - Bu sefer, arkaplan servisini aynı projede yazmak yerine Worker Service kullanıldı.
  - Ayrıca yapılan istekler, SignalR ile birlikte kullanıcıya anlık bir şekilde bildirimler atmaktadır.
