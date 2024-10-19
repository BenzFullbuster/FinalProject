//using System.Net;
//using System.Net.Mail;

//namespace FinalProject.Helpers
//{
//    public class SendEmailHelper
//    {
//        private readonly string _smtpServer;
//        private readonly int _smtpPort;
//        private readonly string _smtpUser;
//        private readonly string _smtpPass;

//        public SendEmailHelper(string smtpServer, int smtpPort, string smtpUser, string smtpPass)
//        {
//            _smtpServer = smtpServer;
//            _smtpPort = smtpPort;
//            _smtpUser = smtpUser;
//            _smtpPass = smtpPass;
//        }

//        public async Task<bool> SendEmailReretPassword(string toEmail, string userName, string resetLink)
//        {
//            try
//            {
//                Console.WriteLine($"เริ่มส่งอีเมลไปยัง {toEmail}");
//                var subject = "ลิงก์สำหรับเปลี่ยนรหัสผ่านของคุณ";
//                var body = $@"<h2>สวัสดี {userName}
//                            <p>คุณได้ขอรีเซ็ตรหัสผ่านสำหรับบัญชีของคุณที่ กรุณาคลิกที่ลิงก์ด้านล่างเพื่อตั้งค่ารหัสผ่านใหม่:</p>
//                            <p><a href='{resetLink}'></a>รีเซ็ตรหัสผ่าน</p>
//                            <p>หากคุณไม่ได้ขอรีเซ็ตรหัสผ่านนี้ กรุณามองข้ามอีเมลนี้</p>
//                            <p>ขอขอบคุณ</p>";

//                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
//                {
//                    smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
//                    smtpClient.EnableSsl = true;

//                    var mailMessage = new MailMessage
//                    {
//                        From = new MailAddress(_smtpUser),
//                        Subject = subject,
//                        Body = body,
//                        IsBodyHtml = true
//                    };

//                    mailMessage.To.Add(toEmail);

//                    await smtpClient.SendMailAsync(mailMessage);
//                    Console.WriteLine($"ส่งอีเมลไปยังสำเร็จ {toEmail}");
//                    return true;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"เกิดข้อผิดพลาดในการส่งอีเมลไปยัง {toEmail}", ex);
//                return false;
//            }
//        }
//    }
//}
