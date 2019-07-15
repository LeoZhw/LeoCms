using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeoCms.Models;

namespace Leo.UnitTest
{
    [TestClass]
    public class ErrorViewModelTest
    {
        [TestMethod]
        public void TestRequestId()
        {
            //����
            ErrorViewModel error = new ErrorViewModel();
            //ִ��
            error.RequestId = "200";
            //����
            Assert.AreEqual(error.RequestId, "200");
        }
    }
}
