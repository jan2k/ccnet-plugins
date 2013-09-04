using System;
using System.Xml;
using System.Collections;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using Exortech.NetReflector;

namespace AZCCTasks
{
	[ReflectorType("BuildGame")]
	public class BuildGamePublisher:ITask
	{
		[ReflectorProperty("resultsFile")]
		public string resultsFile {get;set;}

		XmlDocument gameResults = new XmlDocument();

		public void Run(IIntegrationResult result)
		{
			try {
				gameResults.Load(resultsFile);
			}
			catch (Exception) {
				gameResults.LoadXml(@"<?xml version=""1.0""?><BuildGame project=" + result.ProjectName + "/>");
			}
			
			AddSuccess(result);
			AddWarnings(result);
			AddFailure(result);
			
			gameResults.Save(resultsFile);
		}

		void AddSuccess(IIntegrationResult result) {
			if (result.Status != IntegrationStatus.Success) return;
			string counted = String.Empty;
			foreach(Modification modification in result.Modifications) {
				if (!counted.Contains(modification.UserName)) {
					ChangeScore(modification.UserName, 1);
					counted = counted + modification.UserName;
				}
			}
		}
		
		void AddFailure(IIntegrationResult result) {
			if (result.Status != IntegrationStatus.Failure) return;
			if (result.LastBuildStatus == IntegrationStatus.Success)
				foreach(string user in result.FailureUsers) {
					ChangeScore(user, -10);
			}
		}
		
		void AddWarnings(IIntegrationResult result){
			if (result.Status != IntegrationStatus.Success) return;

		}
		
		void ChangeScore(string user, int change) {
			XmlNode node = gameResults.SelectSingleNode("/BuildGame/user[@name='"+user+"']");
			if (node != null) {
				node.Attributes["score"].Value = (int.Parse(node.Attributes["score"].Value)+change).ToString();
			} else {
				node = gameResults.CreateElement("user");
				XmlAttribute username = gameResults.CreateAttribute("name");
				username.Value = user;
				node.Attributes.Append(username);
				XmlAttribute score = gameResults.CreateAttribute("score");
				score.Value = change.ToString();
				node.Attributes.Append(score);
				gameResults.SelectSingleNode("/BuildGame").AppendChild(node);
			}
		}
	}
	
}