using Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using Utilities.Response;

namespace UnitTest
{
    [TestClass]
    public class DevOps_Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestMethod]
        public void CuandoElObjetoDeEntradaEsNulo()
        {
            // Arrange
            var mock = new Mock<Business.Security.Interface.IDevOps>();
            mock.Setup(s => s.SendMessage(It.IsAny<Entities.RequestMessage>())).Returns(ManagerResponse<bool>.ResponseOk(""));
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(null).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public void CuandoElObjetoDeEntradaEsVacio()
        {
            // Arrange
            var mock = new Entities.RequestMessage()
            {

            };
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(mock).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public void CuandoSoloSenEnviaElAtributoFrom()
        {
            // Arrange
            var mock = new Entities.RequestMessage()
            {
                From = "Usuario "
            };
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(mock).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public void CuandoElObjetoEsOk()
        {
            // Arrange
            var mock = new Entities.RequestMessage()
            {
                From = "Usuario 1",
                Message = "Mensaje",
                TimeToLifeSec = 45,
                To = "Usuario 2"
            };
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(mock).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.OK, result);
        }

        [TestMethod]
        public void CuandoSoloSenEnviaElAtributoTo()
        {
            // Arrange
            var mock = new Entities.RequestMessage()
            {
                To = "Usuario "
            };
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(mock).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public void CuandoSoloSenEnviaElAtributoMessage()
        {
            // Arrange
            var mock = new Entities.RequestMessage()
            {
                Message = "Mensaje",
            };
            // Act
            var classTest = new DevOps();
            var result = classTest.SendMessage(mock).CodigoRespuesta;
            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.InternalServerError, result);
        }
    }
}