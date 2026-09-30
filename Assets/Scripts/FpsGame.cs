using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FpsGame : MonoBehaviour
{
    [Header("Game")]
    public int enemyCount = 6;
    public float arenaSize = 45f;

    private GameObject player;
    private Camera playerCamera;
    private PlayerData playerData;
    private readonly List<EnemyData> enemies = new();
    private GUIStyle labelStyle;
    private GUIStyle centerStyle;
    private float nextShot;
    private int ammo = 30;
    private int reserveAmmo = 120;
    private bool reloading;
    private float reloadEnds;
    private const int MagazineSize = 30;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        BuildScene();
    }

    private void Update()
    {
        if (player == null) return;
        if (playerData.health <= 0 || EnemiesAlive() == 0)
        {
            if (Input.GetKeyDown(KeyCode.Return)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        UpdatePlayer();
        UpdateEnemies();
        if (reloading && Time.time >= reloadEnds) FinishReload();
        if (Input.GetKeyDown(KeyCode.R)) BeginReload();
        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    private void BuildScene()
    {
        RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.35f);
        CreatePrimitive(PrimitiveType.Plane, "Ground", new Vector3(0, 0, 0), new Vector3(arenaSize / 10f, 1, arenaSize / 10f), new Color(0.18f, 0.22f, 0.18f));
        CreatePrimitive(PrimitiveType.Cube, "NorthWall", new Vector3(0, 2, arenaSize / 2), new Vector3(arenaSize, 4, 1), Color.gray);
        CreatePrimitive(PrimitiveType.Cube, "SouthWall", new Vector3(0, 2, -arenaSize / 2), new Vector3(arenaSize, 4, 1), Color.gray);
        CreatePrimitive(PrimitiveType.Cube, "EastWall", new Vector3(arenaSize / 2, 2, 0), new Vector3(1, 4, arenaSize), Color.gray);
        CreatePrimitive(PrimitiveType.Cube, "WestWall", new Vector3(-arenaSize / 2, 2, 0), new Vector3(1, 4, arenaSize), Color.gray);
        CreatePrimitive(PrimitiveType.Cube, "CoverA", new Vector3(0, 1, 5), new Vector3(8, 2, 2), new Color(0.25f, 0.28f, 0.3f));
        CreatePrimitive(PrimitiveType.Cube, "CoverB", new Vector3(-12, 1, -7), new Vector3(2, 2, 9), new Color(0.25f, 0.28f, 0.3f));
        CreatePrimitive(PrimitiveType.Cube, "CoverC", new Vector3(13, 1, -12), new Vector3(9, 2, 2), new Color(0.25f, 0.28f, 0.3f));
        CreatePlayer();
        Vector3[] spots = { new(-16, 1, 15), new(15, 1, 15), new(-17, 1, -16), new(17, 1, -4), new(8, 1, 10), new(-6, 1, -15) };
        for (int i = 0; i < enemyCount && i < spots.Length; i++) CreateEnemy(spots[i]);
    }

    private void CreatePlayer()
    {
        player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1, -16);
        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f; controller.radius = .35f; controller.center = new Vector3(0, .9f, 0);
        playerData = player.AddComponent<PlayerData>();
        playerData.health = 100;
        playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
        playerCamera.transform.SetParent(player.transform); playerCamera.transform.localPosition = new Vector3(0, 1.55f, 0);
        playerCamera.fieldOfView = 75;
    }

    private void CreateEnemy(Vector3 position)
    {
        GameObject go = CreatePrimitive(PrimitiveType.Capsule, "Enemy", position, Vector3.one, new Color(.65f, .12f, .08f));
        var data = go.AddComponent<EnemyData>(); data.health = 100; data.nextAttack = Time.time + Random.Range(1f, 3f); enemies.Add(data);
    }

    private GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.position = position; go.transform.localScale = scale;
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.color = color; go.GetComponent<Renderer>().material = material; return go;
    }

    private void UpdatePlayer()
    {
        float yaw = Input.GetAxis("Mouse X") * 2.5f; float pitch = Input.GetAxis("Mouse Y") * 2.5f;
        player.transform.Rotate(0, yaw, 0); Vector3 cameraAngles = playerCamera.transform.localEulerAngles; float currentPitch = cameraAngles.x > 180 ? cameraAngles.x - 360 : cameraAngles.x; currentPitch = Mathf.Clamp(currentPitch - pitch, -85, 85); playerCamera.transform.localEulerAngles = new Vector3(currentPitch, 0, 0);
        float x = Input.GetAxisRaw("Horizontal"), z = Input.GetAxisRaw("Vertical"); Vector3 move = (player.transform.right * x + player.transform.forward * z).normalized; float speed = Input.GetKey(KeyCode.LeftShift) ? 8f : 5f; player.GetComponent<CharacterController>().Move(move * speed * Time.deltaTime + Vector3.down * 9.8f * Time.deltaTime);
        if (Input.GetMouseButton(0) && Time.time >= nextShot && !reloading) Shoot();
    }

    private void Shoot()
    {
        if (ammo <= 0) { BeginReload(); return; }
        ammo--; nextShot = Time.time + .11f; Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) { var enemy = hit.collider.GetComponent<EnemyData>(); if (enemy != null) enemy.health -= 34; }
    }

    private void BeginReload() { if (!reloading && ammo < MagazineSize && reserveAmmo > 0) { reloading = true; reloadEnds = Time.time + 1.7f; } }
    private void FinishReload() { int amount = Mathf.Min(MagazineSize - ammo, reserveAmmo); ammo += amount; reserveAmmo -= amount; reloading = false; }

    private void UpdateEnemies()
    {
        foreach (var enemy in enemies) { if (enemy == null || enemy.health <= 0) continue; float distance = Vector3.Distance(enemy.transform.position, player.transform.position); if (distance < 28) { Vector3 direction = (player.transform.position - enemy.transform.position); direction.y = 0; if (distance > 2.2f) enemy.transform.position += direction.normalized * 2.1f * Time.deltaTime; else if (Time.time >= enemy.nextAttack) { playerData.health -= 10; enemy.nextAttack = Time.time + 1.1f; } if (direction.sqrMagnitude > .01f) enemy.transform.rotation = Quaternion.LookRotation(direction); } }
    }

    private int EnemiesAlive() { int count = 0; foreach (var e in enemies) if (e != null && e.health > 0) count++; return count; }

    private void OnGUI()
    {
        if (labelStyle == null) { labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, normal = { textColor = Color.white } }; centerStyle = new GUIStyle(labelStyle) { fontSize = 34, alignment = TextAnchor.MiddleCenter }; }
        GUI.Label(new Rect(20, 20, 400, 30), $"生命 {Mathf.Max(0, playerData == null ? 0 : playerData.health)}    敌人 {EnemiesAlive()}", labelStyle);
        GUI.Label(new Rect(20, 48, 400, 30), reloading ? "换弹中..." : $"弹药 {ammo}/{reserveAmmo}   [R]换弹", labelStyle);
        GUI.Label(new Rect(Screen.width / 2 - 10, Screen.height / 2 - 18, 20, 35), "+", centerStyle);
        if (playerData != null && playerData.health <= 0) GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 100), "任务失败\n按 Enter 重新开始", centerStyle);
        else if (player != null && EnemiesAlive() == 0) GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 100), "任务完成\n按 Enter 重新开始", centerStyle);
    }

    private class PlayerData : MonoBehaviour { public int health; }
    private class EnemyData : MonoBehaviour { public int health; public float nextAttack; }
}
