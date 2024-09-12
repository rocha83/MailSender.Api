using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Rochas.Extensions;
using Rochas.Extensions.ValueObjects;
using Rochas.MailSender.Api.Models.ValueObjects;
using Rochas.Net.Connectivity.ValueObjects;

namespace Rochas.MailSender.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[ProducesResponseType(typeof(MailSendResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public class MailSenderController : Controller
	{
		IConfiguration _config;
		string _smtpPwd;
		public MailSenderController(IConfiguration config)
		{
			_config = config;

			var crypto = new Security.Encryption.Encrypter(_config["RochasCryptoKey"], _config["RochasCryptoVector"]);
			_smtpPwd = crypto.DecryptAsText(_config["RochasSmtpPwd"]);
		}

		[HttpPost]
		public async Task<IActionResult> SendMessage(ContactRequest request)
		{
			MailSendResult result;
			if (request == null)
				return BadRequest();

			try
			{
				var smtpConfig = GetSmtpConfig();
				using (var mailer = new Net.Connectivity.MailSender(smtpConfig))
				{
					request.Subject = $"{request.Company} - Contact of {request.Name}";
					var phone = request.PhoneNumber.HasValue ? request.PhoneNumber.Value : 0;
					var msgBody = string.Concat("Phone: ", phone.ToPhoneNumber(), Environment.NewLine,
												"E-mail: ", request.Email, Environment.NewLine,
												Environment.NewLine, "Message : ", request.Message);

					result = await mailer.SendMessage(smtpConfig.DefaultSender, request.Subject, msgBody);

					return Ok(result);
				}
			}
			catch (Exception ex)
			{
				var errorResume = ex.GetResume();
				WriteErrorLog(errorResume);

				return RedirectToAction("Error", "Home", new { errorResume });
			}
		}

		private SmtpConfiguration GetSmtpConfig()
		{
			return new SmtpConfiguration()
			{
				SmtpHost = _config["RochasSmtpHost"],
				SmtpPort = int.Parse(_config["RochasSmtpPort"]),
				SmtpUser = _config["RochasSmtpUser"],
				UseSSL = bool.Parse(_config["RochasSmtpUseSSL"]),
				DefaultSender = _config["RochasEmailSender"],

				SmtpPwd = _smtpPwd
			};
		}

		private void WriteErrorLog(ExceptionResume ex)
		{
			var serialEx = JsonSerializer.Serialize(ex);
			var serialFullPath = string.Concat("Rochas.MailSender_", DateTime.Now.ToString("yyyyMMdd_HHmm_fff"), ".log");

			System.IO.File.WriteAllText(serialFullPath, serialEx);
		}
	}
}
