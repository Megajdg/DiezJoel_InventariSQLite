using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;

    private string createUsuarisQuery = "CREATE TABLE IF NOT EXISTS Usuaris (usuariID INTEGER PRIMARY KEY AUTOINCREMENT, nomUsuari TEXT UNIQUE, contrasenya TEXT)";
    private string createInventariQuery = "CREATE TABLE IF NOT EXISTS Inventari (inventariID INTEGER PRIMARY KEY AUTOINCREMENT, usuariID INTEGER UNIQUE, FOREIGN KEY(usuariID) REFERENCES Usuaris(usuariID))";
    private string createObjectesQuery = "CREATE TABLE IF NOT EXISTS Objectes (objecteID INTEGER PRIMARY KEY AUTOINCREMENT, nomObjecte TEXT, descripcio TEXT, esPotApilar INTEGER)";
    private string createEntradesInventariQuery = "CREATE TABLE IF NOT EXISTS EntradesInventari (entradaID INTEGER PRIMARY KEY AUTOINCREMENT, inventariID INTEGER, objecteID INTEGER, quantitat INTEGER, FOREIGN KEY(inventariID) REFERENCES Inventari(inventariID), FOREIGN KEY(objecteID) REFERENCES Objectes(objecteID))";
    private string createEnstadistiquesObjecteQuery = "CREATE TABLE IF NOT EXISTS EstadistiquesObjecte (estadisticaID INTEGER PRIMARY KEY AUTOINCREMENT, objecteID INTEGER, estaMillorada INTEGER, dany INTEGER, quantitatMax INTEGER, cost INTEGER, FOREIGN KEY(objecteID) REFERENCES Objectes(objecteID))";

    void Awake()
    {
        dbPath = "URI=file:" + Application.streamingAssetsPath + "/DiezJoel_InventariDB.sqlite";
        CreateTables();
    }

    private void CreateTables()
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = createUsuarisQuery;
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = createInventariQuery;
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = createObjectesQuery;
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = createEntradesInventariQuery;
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = createEnstadistiquesObjecteQuery;
                dbCommand.ExecuteNonQuery();
            }

            using (IDbCommand checkCmd = dbConnection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM Objectes";
                long count = (long)checkCmd.ExecuteScalar();
                checkCmd.CommandText = "SELECT COUNT(*) FROM EstadistiquesObjecte";
                long count2 = (long)checkCmd.ExecuteScalar();
                if (count > 0 && count2 > 0)
                    return;
            }

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    "INSERT INTO Objectes (nomObjecte, descripcio, esPotApilar) VALUES " +
                    "('M1911', 'Pistola inicial estándar, fiable pero de bajo daño.', 0)," +
                    "('Ray Gun', 'Arma especial icónica que dispara proyectiles de energía explosiva.', 0)," +
                    "('MP40', 'Subfusil clásico, buena cadencia y estabilidad.', 0)," +
                    "('AK-74u', 'Subfusil compacto con daño moderado y buena movilidad.', 0)," +
                    "('M16', 'Fusil de ráfaga precisa, efectivo a media distancia.', 0)," +
                    "('SPAS-12', 'Escopeta potente a corta distancia, ideal para emergencias.', 0)," +
                    "('Galil', 'Fusil de asalto versátil, equilibrado en daño y precisión.', 0)," +
                    "('RPK', 'Ametralladora ligera con gran cargador y daño sostenido.', 0)," +
                    "('Thunder Gun', 'Arma especial que lanza ondas de choque devastadoras.', 0)," +
                    "('Monkey Bomb', 'Explosivo señuelo que atrae zombis antes de detonar.', 1)";
                dbCommand.ExecuteNonQuery();
            }

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    "INSERT INTO EstadistiquesObjecte (objecteID, estaMillorada, dany, quantitatMax, cost) VALUES " +
                    "(1, 0, 20, 8, 0)," + // M1911
                    "(1, 1, 40, 16, 5000)," + // M1911 Mejorada
                    "(2, 0, 1200, 20, 10000)," + // Ray Gun
                    "(2, 1, 2000, 40, 15000)," + // Ray Gun Mejorada
                    "(3, 0, 40, 32, 1300)," + // MP40
                    "(3, 1, 80, 64, 6300)," + // MP40 Mejorada
                    "(4, 0, 45, 30, 1200)," + // AK-74u
                    "(4, 1, 90, 60, 6200)," + // AK-74u Mejorada
                    "(5, 0, 35, 24, 1200)," + // M16
                    "(5, 1, 70, 48, 6200)," + // M16 Mejorada
                    "(6, 0, 160, 8, 1500)," + // SPAS-12
                    "(6, 1, 320, 16, 6500)," + // SPAS-12 Mejorada
                    "(7, 0, 55, 35, 1500)," + // Galil
                    "(7, 1, 110, 70, 6500)," + // Galil Mejorada
                    "(8, 0, 50, 100, 2000)," + // RPK
                    "(8, 1, 100, 200, 7000)," + // RPK Mejorada
                    "(9, 0, 9999, 2, 10000)," + //Thunder Gun
                    "(9, 1, 9999, 4, 15000)," + //Thunder Gun Mejorada
                    "(10, 0, 500, 3, 5000)," + // Monkey Bomb
                    "(10, 1, 800, 3, 10000)"; // Monkey Bomb Mejorada
                dbCommand.ExecuteNonQuery();
            }
        }
    }

    public string RegisterUser(string username, string password)
    {
        if (password.Length < 8)
            return "The password must have at least 8 characters";

        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    "INSERT INTO Usuaris (nomUsuari, contrasenya) VALUES (@user, @pass)";

                dbCommand.Parameters.Add(new SqliteParameter("@user", username));
                dbCommand.Parameters.Add(new SqliteParameter("@pass", password));

                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch
                {
                    return "The user already exists";
                }
            }

            // Obtener el ID del usuario recién creado
            int userID = -1;
            
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT usuariID FROM Usuaris WHERE nomUsuari = @user";
                cmd.Parameters.Add(new SqliteParameter("@user", username));
                userID = Convert.ToInt32(cmd.ExecuteScalar());
                Debug.Log("UserID:" + userID);
            }
            
            // Crear inventario para ese usuario
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Inventari (usuariID) VALUES (@id)";
                cmd.Parameters.Add(new SqliteParameter("@id", userID));
                cmd.ExecuteNonQuery();
            }

            return "OK";
        }
    }

    public bool LoginUser(string username, string password, out string error)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    "SELECT contrasenya FROM Usuaris WHERE nomUsuari = @user";

                dbCommand.Parameters.Add(new SqliteParameter("@user", username));

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        error = "User not found";
                        return false;
                    }

                    string storedPassword = reader.GetString(0);

                    if (storedPassword != password)
                    {
                        error = "Incorrect password";
                        return false;
                    }

                    error = "";
                    return true;
                }
            }
        }
    }

    public int GetInventariID(string username)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT Inventari.inventariID " +
                    "FROM Inventari " +
                    "JOIN Usuaris ON Usuaris.usuariID = Inventari.usuariID " +
                    "WHERE Usuaris.nomUsuari = @user";

                cmd.Parameters.Add(new SqliteParameter("@user", username));

                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }
    }

    public void LoadStatsIntoItem(InventoryItem item, int objecteID, int estaMillorada)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT dany, quantitatMax, cost " +
                    "FROM EstadistiquesObjecte " +
                    "WHERE objecteID = @id AND estaMillorada = @m";

                cmd.Parameters.Add(new SqliteParameter("@id", objecteID));
                cmd.Parameters.Add(new SqliteParameter("@m", estaMillorada));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.dany = reader.GetInt32(0);
                        item.quantitatMax = reader.GetInt32(1);
                        item.cost = reader.GetInt32(2);
                    }
                }
            }
        }
    }

    public InventoryItem GetInventoryItem(int inventariID, int objecteID)
    {
        List<InventoryItem> items = GetInventoryItems(inventariID);
        return items.Find(i => i.objecteID == objecteID);
    }

    public List<InventoryItem> GetInventoryItems(int inventariID)
    {
        List<InventoryItem> items = new List<InventoryItem>();

        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT o.objecteID, o.nomObjecte, o.descripcio, o.esPotApilar, e.quantitat " +
                    "FROM EntradesInventari e " +
                    "JOIN Objectes o ON o.objecteID = e.objecteID " +
                    "WHERE e.inventariID = @inv";

                cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        InventoryItem item = new InventoryItem();

                        // Leer el objecteID guardado
                        int storedID = reader.GetInt32(0);

                        string keyPrefix = inventariID + "_" + storedID;

                        // Detectar si está mejorada
                        int estaMillorada = PlayerPrefs.GetInt("PAP_" + keyPrefix, 0);

                        // Obtener el ID real del arma
                        int realObjecteID = storedID;
                        
                        // Datos base del arma
                        item.objecteID = storedID;
                        item.nomObjecte = reader.GetString(1);
                        item.descripcio = reader.GetString(2);
                        item.esPotApilar = reader.GetInt32(3);
                        item.quantitat = reader.GetInt32(4);

                        // Cargar estadísticas directamente en InventoryItem
                        item.estaMillorada = estaMillorada;
                        LoadStatsIntoItem(item, storedID, estaMillorada);

                        item.municionEnCargador = PlayerPrefs.GetInt("MAG_" + keyPrefix, item.quantitatMax);
                        item.municionEnReserva = PlayerPrefs.GetInt("RES_" + keyPrefix, item.quantitatMax * 10);

                        items.Add(item);
                    }
                }
            }
        }

        return items;
    }

    public void AddItemToInventory(int inventariID, int objecteID, int quantitat)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();

            // Comprobar si el objeto ya está en el inventario
            int currentQuantity = 0;
            using (IDbCommand checkCmd = dbConnection.CreateCommand())
            {
                checkCmd.CommandText =
                    "SELECT quantitat FROM EntradesInventari WHERE inventariID = @inv AND objecteID = @obj";

                checkCmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                checkCmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                object result = checkCmd.ExecuteScalar();
                if (result != null)
                    currentQuantity = Convert.ToInt32(result);
            }

            // Si ya existe actualizar cantidad
            if (currentQuantity > 0)
            {
                using (IDbCommand updateCmd = dbConnection.CreateCommand())
                {
                    updateCmd.CommandText =
                        "UPDATE EntradesInventari SET quantitat = @q WHERE inventariID = @inv AND objecteID = @obj";

                    updateCmd.Parameters.Add(new SqliteParameter("@q", currentQuantity + quantitat));
                    updateCmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                    updateCmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                    updateCmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Si no existe crear entrada nueva
                using (IDbCommand insertCmd = dbConnection.CreateCommand())
                {
                    insertCmd.CommandText =
                        "INSERT INTO EntradesInventari (inventariID, objecteID, quantitat) VALUES (@inv, @obj, @q)";

                    insertCmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                    insertCmd.Parameters.Add(new SqliteParameter("@obj", objecteID));
                    insertCmd.Parameters.Add(new SqliteParameter("@q", quantitat));

                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }

    public void DeleteItemFromInventory(int inventariID, int objecteID)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "DELETE FROM EntradesInventari WHERE inventariID = @inv AND objecteID = @obj";

                cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void AddOrReplaceItem(int inventariID, int objecteID, int quantitat)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();

            // Saber si el objeto es apilable
            bool esApilable = false;

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT esPotApilar FROM Objectes WHERE objecteID = @obj";
                cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                object result = cmd.ExecuteScalar();
                if (result != null)
                    esApilable = Convert.ToInt32(result) == 1;
            }

            // Comprobar si ya existe en el inventario
            int currentQuantity = 0;

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT quantitat FROM EntradesInventari WHERE inventariID = @inv AND objecteID = @obj";

                cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                object result = cmd.ExecuteScalar();
                if (result != null)
                    currentQuantity = Convert.ToInt32(result);
            }

            // Lógica para armas (no apilables)
            if (!esApilable)
            {
                if (currentQuantity > 0)
                {
                    // Ya tienes el arma no hacer nada
                    Debug.Log("Ya tienes esta arma, no se duplica.");
                    return;
                }

                // Insertar arma con cantidad 1
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO EntradesInventari (inventariID, objecteID, quantitat) VALUES (@inv, @obj, 1)";

                    cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                    cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                    cmd.ExecuteNonQuery();
                }

                return;
            }

            // Lógica para objetos apilables (monos)
            if (currentQuantity > 0)
            {
                // Actualizar cantidad
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE EntradesInventari SET quantitat = @q WHERE inventariID = @inv AND objecteID = @obj";

                    cmd.Parameters.Add(new SqliteParameter("@q", quantitat));
                    cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                    cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Insertar nueva entrada
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO EntradesInventari (inventariID, objecteID, quantitat) VALUES (@inv, @obj, @q)";

                    cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                    cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));
                    cmd.Parameters.Add(new SqliteParameter("@q", quantitat));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public void UpgradeInventoryWeapon(int inventariID, int objecteID)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "UPDATE EntradesInventari SET quantitat = quantitat " +
                    "WHERE inventariID = @inv AND objecteID = @obj";

                cmd.Parameters.Add(new SqliteParameter("@inv", inventariID));
                cmd.Parameters.Add(new SqliteParameter("@obj", objecteID));

                cmd.ExecuteNonQuery();
            }
        }

        string keyPrefix = inventariID + "_" + objecteID;

        // Guardar mejora en PlayerPrefs
        PlayerPrefs.SetInt("PAP_" + keyPrefix, 1);
    }

    public List<InventoryItem> GetAllWeapons()
    {
        List<InventoryItem> items = new List<InventoryItem>();

        using (IDbConnection dbConnection = new SqliteConnection(dbPath))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT o.objecteID, o.nomObjecte, o.descripcio, o.esPotApilar " +
                    "FROM Objectes o";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        InventoryItem item = new InventoryItem();
                        item.objecteID = reader.GetInt32(0);
                        item.nomObjecte = reader.GetString(1);
                        item.descripcio = reader.GetString(2);
                        item.esPotApilar = reader.GetInt32(3);

                        items.Add(item);
                    }
                }
            }
        }

        return items;
    }
}