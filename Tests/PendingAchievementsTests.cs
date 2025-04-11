using System.Linq;
using JTuresson.Social;
using NUnit.Framework;

public class PendingAchievementsTests
{
	[Test]
	public void PendingAchievement_AddAchievement()
	{
		var pendingAchievements = new PendingAchievements();
		var droleAchievement = new DroleAchievement("1");
		pendingAchievements.AddAchievement(droleAchievement);


		Assert.IsTrue(pendingAchievements.Pending.Contains(droleAchievement));
	}

	[Test]
	public void PendingAchievement_RemoveAchievement()
	{
		var pendingAchievements = new PendingAchievements();
		var droleAchievement = new DroleAchievement("1");

		pendingAchievements.AddAchievement(droleAchievement);
		pendingAchievements.RemoveAchievement(droleAchievement);

		Assert.IsFalse(pendingAchievements.Pending.Contains(droleAchievement));
	}

	[Test]
	public void PendingAchievement_RemoveAllWithId()
	{
		var pendingAchievements = new PendingAchievements();
		var droleAchievement = new DroleAchievement("1");
		var droleAchievement2 = new DroleAchievement("2");

		pendingAchievements.AddAchievement(droleAchievement);
		pendingAchievements.AddAchievement(droleAchievement2);
		pendingAchievements.RemoveAllWithId(new[] { droleAchievement.id, droleAchievement2.id });

		Assert.IsTrue(pendingAchievements.Pending.Count == 0);
	}

	[Test]
	public void PendingAchievement_ToAndFromString()
	{
		var pendingAchievements = new PendingAchievements();
		var droleAchievement = new DroleAchievement("1");
		var droleAchievement2 = new DroleAchievement("2");

		pendingAchievements.AddAchievement(droleAchievement);
		pendingAchievements.AddAchievement(droleAchievement2);

		var newPendingAchievementsString = pendingAchievements.ToString();
		PendingAchievements newPendingAchievement = PendingAchievements.FromString(newPendingAchievementsString);

		Assert.AreEqual(pendingAchievements.Pending.Count, newPendingAchievement.Pending.Count);
		Assert.IsTrue(newPendingAchievement.Pending.Contains(droleAchievement));
		Assert.IsTrue(newPendingAchievement.Pending.Contains(droleAchievement2));
	}
}