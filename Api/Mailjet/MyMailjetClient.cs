using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;

namespace Api.Mailjet
{
    public class MyMailjetClient : IMailjetClient
    {
        private bool _isSandboxMode;
        private MailjetClient _inner;

        public MyMailjetClient(HttpClient httpClient, bool isSandboxMode = true)
        {
            _inner = new MailjetClient(httpClient);
            _isSandboxMode = isSandboxMode;
        }

        public Task<MailjetResponse> DeleteAsync(MailjetRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<MailjetResponse> GetAsync(MailjetRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<MailjetResponse> PostAsync(MailjetRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<MailjetResponse> PutAsync(MailjetRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TransactionalEmailResponse> SendTransactionalEmailAsync(TransactionalEmail transactionalEmail, bool isSandboxMode = false, bool advanceErrorHandling = true)
        {
            return _inner.SendTransactionalEmailAsync(transactionalEmail, _isSandboxMode, advanceErrorHandling);
        }

        public Task<TransactionalEmailResponse> SendTransactionalEmailsAsync(IEnumerable<TransactionalEmail> transactionalEmails, bool isSandboxMode = false, bool advanceErrorHandling = true)
        {
            return _inner.SendTransactionalEmailsAsync(transactionalEmails, _isSandboxMode, advanceErrorHandling);
        }
    }
}
