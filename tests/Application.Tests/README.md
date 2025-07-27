# Application Tests

Bu proje, Clean Architecture uygulamasının Application katmanı için unit testleri içerir.

## Kullanılan Test Teknolojileri

- **XUnit**: Ana test framework'ü
- **Moq**: Mocking library
- **FluentAssertions**: Test assertions için
- **MockQueryable.Moq**: Entity Framework async operasyonlarını mock'lamak için
- **AutoMapper**: DTO dönüşümleri için

## Test Yapısı

Tests projesinde aşağıdaki yapı kullanılır:

```
tests/
├── Application.Tests/
│   ├── Common/
│   │   ├── ApplicationDbContextFactory.cs  # Mock DbContext factory
│   │   └── MapperFactory.cs                # AutoMapper test configurasyonu
│   └── Users/
│       └── Create/
│           └── CreateUserCommandHandlerTests.cs  # CreateUserCommandHandler testleri
```

## Test Örnekleri

### CreateUserCommandHandler Testleri

Bu test sınıfı aşağıdaki senaryoları kapsar:

1. **Başarılı kullanıcı oluşturma**: Geçerli verilerle kullanıcı oluşturma
2. **Email benzersizlik kontrolü**: Var olan email ile kayıt denemesi
3. **Şifre hash'leme doğrulaması**: Password hasher'ın çağrıldığının kontrolü
4. **Domain event ekleme**: UserCreatedDomainEvent'in eklendiğinin kontrolü
5. **Cancellation token geçirme**: Token'ın doğru şekilde geçirildiğinin kontrolü
6. **Case-insensitive email kontrolü**: Email'in büyük/küçük harf duyarsız kontrolü

## Test Çalıştırma

```bash
# Tüm testleri çalıştır
dotnet test tests/Application.Tests

# Verbose output ile çalıştır
dotnet test tests/Application.Tests --verbosity normal

# Tek bir test sınıfını çalıştır
dotnet test tests/Application.Tests --filter "CreateUserCommandHandlerTests"
```

## Mock Stratejisi

### Entity Framework Mock'lama

Entity Framework async operasyonları için `MockQueryable.Moq` kütüphanesi kullanılır:

```csharp
// Test verileri oluştur
var users = new List<User>();

// Mock DbSet oluştur
var mockUsersDbSet = users.BuildMockDbSet();

// Mock context'e ata
_mockContext.Setup(x => x.Users).Returns(mockUsersDbSet.Object);
```

### Dependencies Mock'lama

Tüm external dependencies (IPasswordHasher, IApplicationDbContext) mock'lanır:

```csharp
// Password hasher mock
_mockPasswordHasher.Setup(x => x.Hash(password))
    .Returns("hashed_password");

// Context save changes mock
_mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(1);
```

## Test Principles

1. **AAA Pattern**: Arrange-Act-Assert pattern kullanılır
2. **Single Responsibility**: Her test tek bir scenario'yu test eder
3. **Independent Tests**: Testler birbirinden bağımsızdır
4. **Meaningful Names**: Test isimleri ne test edildiğini açık şekilde belirtir
5. **Mock Verification**: Mock'ların doğru çağrıldığı verify edilir

## Best Practices

- Test setup'ı constructor'da yapılır
- Mock'lar her test için reset edilir
- Test verileri her test için fresh oluşturulur
- Assertion'larda FluentAssertions kullanılır
- Async operasyonlar properly test edilir
