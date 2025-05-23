﻿using JTuresson.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Jturesson.Social
{
	public class AchievementButton : MonoBehaviour
	{
		[Header("Scene references")] [SerializeField]
		private Image image;

		[SerializeField] private Button button;
		[Header("Settings")] public Sprite googlePlaySprite;
		public Sprite iTunesSprite;
		public Sprite mockSprite;

		private ISocialManager _socialManager;

		private void Awake()
		{
			if (!image || !button)
			{
				Debug.LogError(gameObject.name + " is missing scene references");
			}

			if (_socialManager != null)
			{
				return;
			}

			var s = FindFirstObjectByType<SocialManager>();
			Inject(s);
		}

		private void Start()
		{
			bool isAuthenticated = _socialManager.Authenticated;
			button.gameObject.SetActive(isAuthenticated);
			if (isAuthenticated)
			{
				UpdateImage(_socialManager.Platform);
			}

			_socialManager.AuthenticatedChanged += SocialManager_AuthenticatedChanged;
		}

		private void OnEnable()
		{
			button.onClick.AddListener(OnButtonClicked);
		}

		private void OnDisable()
		{
			button.onClick.RemoveListener(OnButtonClicked);
		}

		private void OnDestroy()
		{
			if (_socialManager != null)
			{
				_socialManager.AuthenticatedChanged -= SocialManager_AuthenticatedChanged;
			}
		}

		public void Inject(ISocialManager socialManager)
		{
			_socialManager = socialManager;
		}

		private void OnButtonClicked()
		{
			if (_socialManager != null)
			{
				_socialManager.Achievements.ShowUI();
			}
		}

		private void SocialManager_AuthenticatedChanged(object sender, SocialManagerArgs e)
		{
			button.gameObject.SetActive(e.Authenticated);
			UpdateImage(e.Platform);
		}

		private void UpdateImage(RuntimePlatform platform)
		{
			if (platform == RuntimePlatform.Android)
			{
				image.sprite = googlePlaySprite;
			}
			else if (platform == RuntimePlatform.IPhonePlayer)
			{
				image.sprite = iTunesSprite;
			}
			else
			{
				image.sprite = mockSprite;
			}
		}
	}
}