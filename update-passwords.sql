-- Script para atualizar as senhas dos usuários
-- Hash BCrypt para Admin@123: $2a$11$r4aHE2PnR4xi9noJxkzqe.2SIC5DqPZvinTi8EmFOHsMRIWcrPkqi
-- Hash BCrypt para Corretor@123: $2a$11$D8sg3FrM1EI689Z905iG2ubYw/m6LSlI3au9TWZFWd9dCFhw9rxQS

USE ImovelStandDb;

UPDATE Usuarios SET SenhaHash = '$2a$11$r4aHE2PnR4xi9noJxkzqe.2SIC5DqPZvinTi8EmFOHsMRIWcrPkqi' WHERE Id = 1;
UPDATE Usuarios SET SenhaHash = '$2a$11$D8sg3FrM1EI689Z905iG2ubYw/m6LSlI3au9TWZFWd9dCFhw9rxQS' WHERE Id = 2;
