IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Brands] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [LogoPath] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
);

CREATE TABLE [Campaigns] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [BannerImagePath] nvarchar(300) NULL,
    [DiscountType] nvarchar(20) NOT NULL,
    [DiscountValue] decimal(18,2) NOT NULL,
    [AppliesToAllProducts] bit NOT NULL,
    [StartsAt] datetime2 NOT NULL,
    [EndsAt] datetime2 NOT NULL,
    [IsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Campaigns] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Campaigns_Dates] CHECK ([EndsAt] > [StartsAt]),
    CONSTRAINT [CK_Campaigns_DiscountType] CHECK ([DiscountType] IN ('Percentage', 'FixedAmount')),
    CONSTRAINT [CK_Campaigns_DiscountValue] CHECK ([DiscountValue] > 0 AND ([DiscountType] <> 'Percentage' OR [DiscountValue] <= 100))
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [ParentId] int NULL,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [ImagePath] nvarchar(300) NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Categories_NotOwnParent] CHECK ([ParentId] IS NULL OR [ParentId] <> [Id]),
    CONSTRAINT [FK_Categories_Categories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DeliveryMethods] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(300) NULL,
    [Price] decimal(18,2) NOT NULL,
    [FreeOverAmount] decimal(18,2) NULL,
    [MinDays] tinyint NOT NULL,
    [MaxDays] tinyint NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_DeliveryMethods] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DeliveryMethods_Days] CHECK ([MinDays] <= [MaxDays]),
    CONSTRAINT [CK_DeliveryMethods_Price] CHECK ([Price] >= 0)
);

CREATE TABLE [ProductAttributes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ProductAttributes] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(30) NULL,
    [Role] nvarchar(20) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LastLoginAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Customer', 'StoreManager', 'Admin'))
);

CREATE TABLE [CampaignCategories] (
    [CampaignId] int NOT NULL,
    [CategoryId] int NOT NULL,
    CONSTRAINT [PK_CampaignCategories] PRIMARY KEY ([CampaignId], [CategoryId]),
    CONSTRAINT [FK_CampaignCategories_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Slug] nvarchar(220) NOT NULL,
    [Sku] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CategoryId] int NOT NULL,
    [BrandId] int NULL,
    [StockQuantity] int NOT NULL,
    [LowStockThreshold] int NOT NULL DEFAULT 5,
    [Status] nvarchar(20) NOT NULL,
    [IsDeleted] bit NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Products_LowStockThreshold] CHECK ([LowStockThreshold] >= 0),
    CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_Products_Status] CHECK ([Status] IN ('Active', 'Inactive')),
    CONSTRAINT [CK_Products_StockQuantity] CHECK ([StockQuantity] >= 0),
    CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Addresses] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Label] nvarchar(50) NOT NULL,
    [RecipientName] nvarchar(150) NOT NULL,
    [Phone] nvarchar(30) NOT NULL,
    [Line1] nvarchar(200) NOT NULL,
    [Line2] nvarchar(200) NULL,
    [City] nvarchar(100) NOT NULL,
    [Region] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [CountryCode] char(2) NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Addresses_CountryCode] CHECK (LEN([CountryCode]) = 2),
    CONSTRAINT [FK_Addresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Carts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Carts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountTotal] decimal(18,2) NOT NULL,
    [ShippingCost] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [DeliveryMethodId] int NOT NULL,
    [PaymentMethod] nvarchar(30) NOT NULL,
    [CustomerNote] nvarchar(500) NULL,
    [PaidAt] datetime2 NULL,
    [ShipCity] nvarchar(100) NOT NULL,
    [ShipCountryCode] varchar(2) NOT NULL,
    [ShipLine1] nvarchar(200) NOT NULL,
    [ShipLine2] nvarchar(200) NULL,
    [ShipPhone] nvarchar(30) NOT NULL,
    [ShipPostalCode] nvarchar(20) NULL,
    [ShipRecipientName] nvarchar(150) NOT NULL,
    [ShipRegion] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Orders_Amounts] CHECK ([Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [ShippingCost] >= 0 AND [TaxAmount] >= 0),
    CONSTRAINT [CK_Orders_PaymentMethod] CHECK ([PaymentMethod] IN ('CashOnDelivery', 'Card')),
    CONSTRAINT [CK_Orders_Status] CHECK ([Status] IN ('Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT [CK_Orders_Total] CHECK ([TotalAmount] = [Subtotal] - [DiscountTotal] + [ShippingCost] + [TaxAmount]),
    CONSTRAINT [FK_Orders_DeliveryMethods_DeliveryMethodId] FOREIGN KEY ([DeliveryMethodId]) REFERENCES [DeliveryMethods] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PasswordResetTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [TokenHash] char(64) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [UsedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PasswordResetTokens_Expiry] CHECK ([ExpiresAt] > [CreatedAt]),
    CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CampaignProducts] (
    [CampaignId] int NOT NULL,
    [ProductId] int NOT NULL,
    CONSTRAINT [PK_CampaignProducts] PRIMARY KEY ([CampaignId], [ProductId]),
    CONSTRAINT [FK_CampaignProducts_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductAttributeValues] (
    [ProductId] int NOT NULL,
    [ProductAttributeId] int NOT NULL,
    [Value] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ProductAttributeValues] PRIMARY KEY ([ProductId], [ProductAttributeId]),
    CONSTRAINT [FK_ProductAttributeValues_ProductAttributes_ProductAttributeId] FOREIGN KEY ([ProductAttributeId]) REFERENCES [ProductAttributes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductAttributeValues_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductImages] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [FilePath] nvarchar(300) NOT NULL,
    [AltText] nvarchar(200) NULL,
    [SortOrder] int NOT NULL,
    [IsMain] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductPriceHistory] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [OldPrice] decimal(18,2) NOT NULL,
    [NewPrice] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ProductPriceHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductPriceHistory_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Reviews] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UserId] int NOT NULL,
    [Rating] tinyint NOT NULL,
    [Title] nvarchar(150) NULL,
    [Comment] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Reviews_Rating] CHECK ([Rating] BETWEEN 1 AND 5),
    CONSTRAINT [FK_Reviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [WishlistItems] (
    [UserId] int NOT NULL,
    [ProductId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_WishlistItems] PRIMARY KEY ([UserId], [ProductId]),
    CONSTRAINT [FK_WishlistItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WishlistItems_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CartItems] (
    [Id] int NOT NULL IDENTITY,
    [CartId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CartItems_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(30) NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [ProductId] int NULL,
    [OrderId] int NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Notifications_Type] CHECK ([Type] IN ('LowStock', 'OutOfStock', 'NewOrder')),
    CONSTRAINT [FK_Notifications_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Notifications_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [OrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [Sku] nvarchar(50) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [UnitDiscount] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    [LineTotal] AS ([UnitPrice] - [UnitDiscount]) * [Quantity] PERSISTED,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderItems_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_OrderItems_UnitDiscount] CHECK ([UnitDiscount] >= 0 AND [UnitDiscount] <= [UnitPrice]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [OrderStatusHistory] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [FromStatus] nvarchar(20) NULL,
    [ToStatus] nvarchar(20) NOT NULL,
    [ChangedByUserId] int NULL,
    [Note] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_OrderStatusHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderStatusHistory_ToStatus] CHECK ([ToStatus] IN ('Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT [FK_OrderStatusHistory_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderStatusHistory_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Addresses_UserId] ON [Addresses] ([UserId]);

CREATE UNIQUE INDEX [UQ_Addresses_UserId_Default] ON [Addresses] ([UserId]) WHERE [IsDefault] = 1;

CREATE UNIQUE INDEX [UQ_Brands_Name] ON [Brands] ([Name]);

CREATE UNIQUE INDEX [UQ_Brands_Slug] ON [Brands] ([Slug]);

CREATE INDEX [IX_CampaignCategories_CategoryId] ON [CampaignCategories] ([CategoryId]);

CREATE INDEX [IX_CampaignProducts_ProductId] ON [CampaignProducts] ([ProductId]);

CREATE INDEX [IX_Campaigns_Active] ON [Campaigns] ([IsEnabled], [StartsAt], [EndsAt]);

CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);

CREATE UNIQUE INDEX [UQ_CartItems_CartId_ProductId] ON [CartItems] ([CartId], [ProductId]);

CREATE UNIQUE INDEX [IX_Carts_UserId] ON [Carts] ([UserId]);

CREATE UNIQUE INDEX [UQ_Categories_ParentId_Name] ON [Categories] ([ParentId], [Name]);

CREATE UNIQUE INDEX [UQ_Categories_Slug] ON [Categories] ([Slug]);

CREATE UNIQUE INDEX [UQ_DeliveryMethods_Name] ON [DeliveryMethods] ([Name]);

CREATE INDEX [IX_Notifications_IsRead_CreatedAt] ON [Notifications] ([IsRead], [CreatedAt] DESC);

CREATE INDEX [IX_Notifications_OrderId] ON [Notifications] ([OrderId]);

CREATE INDEX [IX_Notifications_ProductId] ON [Notifications] ([ProductId]);

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);

CREATE UNIQUE INDEX [UQ_OrderItems_OrderId_ProductId] ON [OrderItems] ([OrderId], [ProductId]);

CREATE INDEX [IX_Orders_CreatedAt] ON [Orders] ([CreatedAt]) INCLUDE ([TotalAmount], [Status]);

CREATE INDEX [IX_Orders_DeliveryMethodId] ON [Orders] ([DeliveryMethodId]);

CREATE INDEX [IX_Orders_Status_CreatedAt] ON [Orders] ([Status], [CreatedAt]);

CREATE INDEX [IX_Orders_UserId_CreatedAt] ON [Orders] ([UserId], [CreatedAt] DESC);

CREATE INDEX [IX_OrderStatusHistory_ChangedByUserId] ON [OrderStatusHistory] ([ChangedByUserId]);

CREATE INDEX [IX_OrderStatusHistory_OrderId_CreatedAt] ON [OrderStatusHistory] ([OrderId], [CreatedAt]);

CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);

CREATE UNIQUE INDEX [UQ_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);

CREATE UNIQUE INDEX [UQ_ProductAttributes_Name] ON [ProductAttributes] ([Name]);

CREATE INDEX [IX_ProductAttributeValues_AttributeId_Value] ON [ProductAttributeValues] ([ProductAttributeId], [Value]);

CREATE INDEX [IX_ProductImages_ProductId_SortOrder] ON [ProductImages] ([ProductId], [SortOrder]);

CREATE UNIQUE INDEX [UQ_ProductImages_ProductId_Main] ON [ProductImages] ([ProductId]) WHERE [IsMain] = 1;

CREATE INDEX [IX_ProductPriceHistory_ProductId_CreatedAt] ON [ProductPriceHistory] ([ProductId], [CreatedAt]);

CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]) INCLUDE ([Price], [Status]);

CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);

CREATE INDEX [IX_Products_Price] ON [Products] ([Price]);

CREATE UNIQUE INDEX [UQ_Products_Sku] ON [Products] ([Sku]);

CREATE UNIQUE INDEX [UQ_Products_Slug] ON [Products] ([Slug]);

CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);

CREATE UNIQUE INDEX [UQ_Reviews_ProductId_UserId] ON [Reviews] ([ProductId], [UserId]);

CREATE UNIQUE INDEX [UQ_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_WishlistItems_ProductId] ON [WishlistItems] ([ProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261117113341_InitialCreate', N'10.0.15');

COMMIT;
GO

