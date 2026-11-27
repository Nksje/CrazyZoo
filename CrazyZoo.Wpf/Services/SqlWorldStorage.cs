using System.Data;
using System.IO;
using CrazyZoo.Core.Persistence;
using Microsoft.Data.SqlClient;

namespace CrazyZoo.Wpf.Services;

// Maailma salvestamine LocalDB-sse (ADO.NET).
// path = maailma nimi andmebaasis (nt "default"), sest leping on sama mis failidel.
public class SqlWorldStorage : IWorldStorage
{
    private readonly string _connectionString;

    public SqlWorldStorage(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(token);

        // Maailm, rühmad ja loomad salvestatakse ühe tehinguna
        using SqlTransaction tx = conn.BeginTransaction();
        try
        {
            int worldId = await SaveWorldRowAsync(conn, tx, path, world.Version, token);
            await DeleteOldRowsAsync(conn, tx, worldId, token);

            foreach (GroupDto group in world.Groups)
                await InsertGroupAsync(conn, tx, worldId, group, token);

            foreach (AnimalDto animal in world.Animals)
                await InsertAnimalAsync(conn, tx, worldId, animal, token);

            tx.Commit(); // alles nüüd on kõik muudatused kinnitatud
        }
        catch
        {
            tx.Rollback(); // pool salvestust ei jää andmebaasi
            throw;
        }
    }

    public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(token);

        var world = new WorldDto();
        int worldId;

        // 1) Maailm
        await using (var cmd = new SqlCommand("SELECT Id, Version FROM dbo.Worlds WHERE Name = @name;", conn))
        {
            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = path;
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(token);
            if (!await reader.ReadAsync(token))
                throw new InvalidDataException($"Andmebaasis pole maailma nimega '{path}'.");
            worldId = reader.GetInt32(0);
            world.Version = reader.GetInt32(1);
        }

        // 2) Rühmad
        await using (var cmd = new SqlCommand(
            "SELECT Id, Name, AnimalType FROM dbo.AnimalGroups WHERE WorldId = @w ORDER BY Id;", conn))
        {
            cmd.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                world.Groups.Add(new GroupDto
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AnimalType = reader.GetString(2)
                });
            }
        }

        // 3) Loomad
        await using (var cmd = new SqlCommand(
            "SELECT Id, Type, Name, Energy, Hunger, Mood, ReactionCount, GroupId " +
            "FROM dbo.Animals WHERE WorldId = @w ORDER BY Id;", conn))
        {
            cmd.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                world.Animals.Add(new AnimalDto
                {
                    Id = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    Name = reader.GetString(2),
                    Energy = reader.GetInt32(3),
                    Hunger = reader.GetInt32(4),
                    Mood = reader.GetInt32(5),
                    ReactionCount = reader.GetInt32(6),
                    GroupId = reader.GetInt32(7)
                });
            }
        }

        // Kontrolli ja maailma loomise teeb sama WorldMapper.FromDto nagu failide puhul
        return world;
    }

    // Leiab maailma rea nime järgi või loob uue. Tagastab maailma Id.
    private static async Task<int> SaveWorldRowAsync(SqlConnection conn, SqlTransaction tx,
        string name, int version, CancellationToken token)
    {
        await using (var find = new SqlCommand("SELECT Id FROM dbo.Worlds WHERE Name = @name;", conn, tx))
        {
            find.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
            object? found = await find.ExecuteScalarAsync(token);

            if (found != null)
            {
                int existingId = (int)found;
                await using var update = new SqlCommand(
                    "UPDATE dbo.Worlds SET Version = @v, SavedAt = SYSUTCDATETIME() WHERE Id = @id;", conn, tx);
                update.Parameters.Add("@v", SqlDbType.Int).Value = version;
                update.Parameters.Add("@id", SqlDbType.Int).Value = existingId;
                await update.ExecuteNonQueryAsync(token);
                return existingId;
            }
        }

        await using var insert = new SqlCommand(
            "INSERT INTO dbo.Worlds (Name, Version, SavedAt) OUTPUT INSERTED.Id " +
            "VALUES (@name, @v, SYSUTCDATETIME());", conn, tx);
        insert.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
        insert.Parameters.Add("@v", SqlDbType.Int).Value = version;
        object? newId = await insert.ExecuteScalarAsync(token);
        return (int)newId!;
    }

    // Selle maailma vanad loomad ja rühmad kustutatakse (enne loomad, sest neil on välisvõti rühmale)
    private static async Task DeleteOldRowsAsync(SqlConnection conn, SqlTransaction tx,
        int worldId, CancellationToken token)
    {
        await using (var deleteAnimals = new SqlCommand("DELETE FROM dbo.Animals WHERE WorldId = @w;", conn, tx))
        {
            deleteAnimals.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
            await deleteAnimals.ExecuteNonQueryAsync(token);
        }

        await using (var deleteGroups = new SqlCommand("DELETE FROM dbo.AnimalGroups WHERE WorldId = @w;", conn, tx))
        {
            deleteGroups.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
            await deleteGroups.ExecuteNonQueryAsync(token);
        }
    }

    private static async Task InsertGroupAsync(SqlConnection conn, SqlTransaction tx,
        int worldId, GroupDto group, CancellationToken token)
    {
        await using var cmd = new SqlCommand(
            "INSERT INTO dbo.AnimalGroups (WorldId, Id, Name, AnimalType) " +
            "VALUES (@w, @id, @name, @type);", conn, tx);
        cmd.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = group.Id;
        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = group.Name;
        cmd.Parameters.Add("@type", SqlDbType.NVarChar, 20).Value = group.AnimalType;
        await cmd.ExecuteNonQueryAsync(token);
    }

    private static async Task InsertAnimalAsync(SqlConnection conn, SqlTransaction tx,
        int worldId, AnimalDto animal, CancellationToken token)
    {
        await using var cmd = new SqlCommand(
            "INSERT INTO dbo.Animals (WorldId, Id, Type, Name, Energy, Hunger, Mood, ReactionCount, GroupId) " +
            "VALUES (@w, @id, @type, @name, @energy, @hunger, @mood, @reactions, @group);", conn, tx);
        cmd.Parameters.Add("@w", SqlDbType.Int).Value = worldId;
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = animal.Id;
        cmd.Parameters.Add("@type", SqlDbType.NVarChar, 20).Value = animal.Type;
        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 20).Value = animal.Name;
        cmd.Parameters.Add("@energy", SqlDbType.Int).Value = animal.Energy;
        cmd.Parameters.Add("@hunger", SqlDbType.Int).Value = animal.Hunger;
        cmd.Parameters.Add("@mood", SqlDbType.Int).Value = animal.Mood;
        cmd.Parameters.Add("@reactions", SqlDbType.Int).Value = animal.ReactionCount;
        cmd.Parameters.Add("@group", SqlDbType.Int).Value = animal.GroupId;
        await cmd.ExecuteNonQueryAsync(token);
    }
}
