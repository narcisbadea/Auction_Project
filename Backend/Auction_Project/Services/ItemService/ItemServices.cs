using Auction_Project.DAL;
using Auction_Project.DataBase;
using Auction_Project.Models.Bids;
using Auction_Project.Models.Items;
using Auction_Project.Models.Pictures;
using Auction_Project.Models.Users;
using Auction_Project.Services.BidService;
using Auction_Project.Services.EmailService;
using Auction_Project.Services.UserService;
using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;


namespace Auction_Project.Services.ItemService;

public class ItemsServices
{
    private readonly AppDbContext _context;
    private readonly IRepository<Item> _repositoryItems;
    private readonly IRepositoryItem _repositoryItemCustom;
    private readonly IRepositoryUser _repositoryUser;
    private readonly IRepositoryBids _repositoryBids;
    private readonly IRepositoryPictures _repositoryPictures;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IBidCloseServices _bidCloseServices;
    private readonly IEmailService _emailService;

    public ItemsServices(AppDbContext context, IRepository<Item> repository, IMapper mapper, IRepositoryBids repositoryBids, IRepositoryPictures repositoryPictures, IRepositoryItem repositoryItemCustom, IUserService userService, IBidCloseServices bidCloseServices, IEmailService emailService)
    {
        _context = context;
        _repositoryItems = repository;
        _mapper = mapper;
        _repositoryBids = repositoryBids;
        _repositoryPictures = repositoryPictures;
        _repositoryItemCustom = repositoryItemCustom;
        _userService = userService;
        _bidCloseServices = bidCloseServices;
        _emailService = emailService;
    }

    public async Task<IEnumerable<ItemResponseForClientDTO>> GetUser()
    {
        var items = await _repositoryItemCustom.Get();
        items = items.Where(i => i.Available == true).ToList();
        var bids = await _repositoryBids.Get();

        var response = new List<ItemResponseForClientDTO>();
       
            foreach (var item in items)
            {
            if (item.Available)
            {
                var lastBid = bids.Where(i => i.Item.Id == item.Id).OrderBy(b => b.bidTime).LastOrDefault();
                if (lastBid != null)
                {
                    //var itemResponse = _mapper.Map<ItemResponseDTO>(lastBid.Item);
                    var itemResponse = new ItemResponseDTO
                    {
                        Id = lastBid.Item.Id,
                        Name = lastBid.Item.Name,
                        Desc = lastBid.Item.Desc,

                        Price = lastBid.Item.Price,

                        EndTime = lastBid.Item.postedTime.Value.AddMinutes(Convert.ToInt32(lastBid.Item.EndTime)).ToString(),

                        postedTime = lastBid.Item.postedTime,

                        Gallery = lastBid.Item.Gallery.Select(i => i.Id).ToList()
                    };
                    var userResponse = _mapper.Map<UserResponseDTO>(lastBid.User);
                    var res = new BidResponseDTO
                    {
                        ItemResponse = itemResponse,
                        UserResponse = userResponse,
                        BidPrice = lastBid.BidPrice
                    };

                    var listGalleryIds = new List<int>();
                    if (item.Gallery.Count > 0)
                    {
                        foreach (var pic in item.Gallery)
                        {
                            listGalleryIds.Add(pic.Id);
                        }
                    }
                    else
                    {
                        listGalleryIds.Add(-1);
                    }

                    response.Add(new ItemResponseForClientDTO
                    {
                        Id=item.Id,

                        Name = item.Name,

                        Desc = item.Desc,

                        InitialPrice = item.Price,

                        EndTime = lastBid.Item.postedTime.Value.AddMinutes(Convert.ToInt32(lastBid.Item.EndTime)).ToString(),

                        Gallery = listGalleryIds,

                        LastBidUserFirstName = res.UserResponse.FirstName,

                        LastBidPrice = res.BidPrice
                    });
                }
                else
                {
                    var listGalleryIds = new List<int>();
                    if (item.Gallery.Count > 0)
                    {
                        foreach (var pic in item.Gallery)
                        {
                            listGalleryIds.Add(pic.Id);
                        }
                    }
                    else
                    {
                        listGalleryIds.Add(-1);
                    }
                    response.Add(new ItemResponseForClientDTO
                    {
                        Id = item.Id,

                        Name = item.Name,

                        Desc = item.Desc,

                        InitialPrice = item.Price,

                        EndTime = item.postedTime.Value.AddMinutes(Convert.ToInt32(item.EndTime)).ToString(),


                        Gallery = listGalleryIds,

                        LastBidUserFirstName = "No bidder yet",

                        LastBidPrice = 0
                    });
                }
            }
            }
        return response;
    }

    public async Task<IEnumerable<ItemResponseDTO>> GetUnapprovedForAdmin()
    {
        var items = await _repositoryItemCustom.Get();
        var result = new List<ItemResponseDTO>();
        foreach(var item in items)
        {
            if(item.Available == false && item.IsSold == false)
            {
                var temp = new ItemResponseDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    Desc=item.Desc,
                    Price = item.Price,
                    EndTime=item.EndTime.ToString(),
                    postedTime = item.postedTime,
                    Gallery = item.Gallery.Select(i => i.Id).ToList()
                };
                result.Add(temp);
            }
        }
        return result;
    }

    public async Task<int> GetNumberOfPagesForApprove()
    {
        var items = await GetUnapprovedForAdmin();
        return items.Count();
    }

    public async Task<IEnumerable<ItemResponseForAdminDTO>> GetAdmin()
    {
        var items = await _repositoryItemCustom.Get();
        
        var bids = await _repositoryBids.Get();
        var bidDTO = new List<BidResponseForAdminDTO>();

        foreach (var bid in bids)
        {
            var bidtemp = new BidResponseForAdminDTO
            {
                UserNameForBid = bid.User.UserName,
                UserEmailForBid = bid.User.Email,

                ItemIdForBid = bid.Item.Id,
                ItemIsSold = bid.Item.IsSold,

                Price = bid.BidPrice
            };
            bidDTO.Add(bidtemp);
        }

        var response = new List<ItemResponseForAdminDTO>();
  
        foreach (var item in items)
        {
            var lastBid = bids.Where(i => i.Item.Id == item.Id).OrderBy(b => b.bidTime).LastOrDefault();
            if (lastBid != null)
            {
                var itemResponse = new ItemResponseDTO
                {
                    Id = lastBid.Item.Id,

                    Name = lastBid.Item.Name,
                    Desc = lastBid.Item.Desc,

                    Price = lastBid.Item.Price,

                    EndTime = lastBid.Item.EndTime.ToString(),

                    postedTime = lastBid.Item.postedTime,

                    Gallery = lastBid.Item.Gallery.Select(i => i.Id).ToList()
                };
                var userResponse = _mapper.Map<UserResponseDTO>(lastBid.User);
                var res = new BidResponseDTO
                {
                    ItemResponse = itemResponse,
                    UserResponse = userResponse,
                    BidPrice = lastBid.BidPrice
                };

                var listGalleryIds = new List<int>();
                if (item.Gallery.Count > 0)
                {
                    foreach (var pic in item.Gallery)
                    {
                        listGalleryIds.Add(pic.Id);
                    }
                }
                else
                {
                    listGalleryIds.Add(-1);
                }

                response.Add(new ItemResponseForAdminDTO
                {
                        
                        Id = item.Id,
                        
                        Name = item.Name,

                        Desc = item.Desc,

                        InitialPrice = item.Price,

                        EndTime = item.EndTime,

                        Gallery = listGalleryIds,
                    
                        BidsOnItem = bidDTO.FindAll(bid=>bid.ItemIdForBid==item.Id)
                });
            }
            else
            {
                var listGalleryIds = new List<int>();
                if (item.Gallery.Count > 0)
                {
                    foreach (var pic in item.Gallery)
                    {
                        listGalleryIds.Add(pic.Id);
                    }
                }
                else
                {
                    listGalleryIds.Add(-1);
                }
                response.Add(new ItemResponseForAdminDTO
                {
                    Id = item.Id,

                    Name = item.Name,

                    Desc = item.Desc,

                    InitialPrice = item.Price,

                    EndTime = item.EndTime,

                    Gallery = listGalleryIds,

                    BidsOnItem = null
                });
            }
        }
        return response;
    }

    public async Task<IEnumerable<ItemResponseForClientDTO>> GetUserByPage(int nr)
    {
        var list = await GetUser();
        var maxPage = list.ToList().Count /  5;
        if (list.ToList().Count % 5 > 0)
        {
            maxPage++;
        }
        if (nr <= maxPage)
        {
            var result = list.ToList().GetRange(5 * nr - 5, 5 - ((nr * 5) - list.ToList().Count));
            return result;
        }
        else
        {
            return null;
        }
    }

    public async Task<IEnumerable<ItemResponseForAdminDTO>> GetAdminByPage(int nr)
    {
        var list = await GetAdmin();
        var maxPage = list.ToList().Count / 5;
        if(list.ToList().Count % 5 > 0)
        {
            maxPage++;
        }
        if (nr <= maxPage)
        {
            var result = list.ToList().GetRange(5 * nr - 5, 5 - ((nr * 5) - list.ToList().Count));
            return result;
        }
        else
        {
            return null;
        }
        
    }

    public async Task<IEnumerable<ItemResponseDTO>> GetAdminByPageUnapproved(int nr)
    {
        var list = await GetUnapprovedForAdmin();
        var maxPage = list.ToList().Count / 5;
        if (list.ToList().Count % 5 > 0)
        {
            maxPage++;
        }
        if (nr <= maxPage)
        {
            var result = list.ToList().GetRange(5 * nr - 5, 5 - ((nr * 5) - list.ToList().Count));
            return result;
        }
        else
        {
            return null;
        }

    }

    public async Task<ItemResponseForClientDTO> GetByIdForUser(int id)
    {
        var item = await _repositoryItemCustom.GetById(id);
        var bids = await _repositoryBids.Get();

        if (item != null)
        {
            var lastBid = bids.Where(i => i.Item.Id == item.Id).OrderBy(b => b.bidTime).LastOrDefault();
            if (lastBid != null)
            {
                var itemResponse = new ItemResponseDTO
                {
                    Id = lastBid.Item.Id,
                    Name = lastBid.Item.Name,
                    Desc = lastBid.Item.Desc,

                    Price = lastBid.Item.Price,

                    EndTime = lastBid.Item.EndTime.ToString(),

                    postedTime = lastBid.Item.postedTime,

                    Gallery = lastBid.Item.Gallery.Select(i => i.Id).ToList()
                };
                var userResponse = _mapper.Map<UserResponseDTO>(lastBid.User);
                var res = new BidResponseDTO
                {
                    ItemResponse = itemResponse,
                    UserResponse = userResponse,
                    BidPrice = lastBid.BidPrice
                };
                var listGalleryIds = new List<int>();
                if (item.Gallery.Count > 0)
                {
                    foreach (var pic in item.Gallery)
                    {
                        listGalleryIds.Add(pic.Id);
                    }
                }
                else
                {
                    listGalleryIds.Add(-1);
                }
                var response = new ItemResponseForClientDTO
                {
                    Id =item.Id,
                    Name = item.Name,

                    Desc = item.Desc,

                    InitialPrice = item.Price,

                    EndTime = item.EndTime.ToString(),

                    Gallery = listGalleryIds,

                    LastBidUserFirstName = res.UserResponse.FirstName,

                    LastBidPrice = res.BidPrice
                };
                return response;
            }
            else
            {
                var listGalleryIds = new List<int>();
                if (item.Gallery.Count > 0)
                {
                    foreach (var pic in item.Gallery)
                    {
                        listGalleryIds.Add(pic.Id);
                    }
                }
                else
                {
                    listGalleryIds.Add(-1);
                }
                var response = new ItemResponseForClientDTO
                {
                    Id = item.Id,

                    Name = item.Name,

                    Desc = item.Desc,

                    InitialPrice = item.Price,

                    EndTime = item.EndTime.ToString(),

                    Gallery = listGalleryIds,

                    LastBidUserFirstName = "No bidder yet",

                    LastBidPrice = 0
                };
                return response;
            }
        }
        return null;
    }
    
    public async Task<ItemResponseDTO> GetById(int id)
    {
        return _mapper.Map<ItemResponseDTO>(await _repositoryItems.GetById(id));
    }

    public async Task<string?> GetItemState(int itemId)
    {
        var item = await _repositoryItems.GetById(itemId);
        if(item != null)
        {
            if (item.IsSold)
            {
                return "sold";
            }
            else
            {
                if (item.Available)
                {
                    return "listed";
                }
                else
                {
                    return "unlisted";
                }
            }
        }
        return null;
    }

    public async Task<ItemResponseForClientDTO> PostClient(ItemRequestDTO item)
    {
        var picList = new List<Picture>();
        var getLoggedUser = _userService.GetMe();

        foreach (var gallryId in item.GalleryIds)
        {
            picList.Add(await _repositoryPictures.GetById(gallryId));
        }

        var toPost = new Item
        {

            Name = item.Name,

            IsSold = false,

            OwnerUserId = getLoggedUser.Result.Id,

            Available = false,

            Desc = item.Desc,

            Price = item.Price,

            winningBidId = null,

            EndTime = item.EndTime,

            postedTime = DateTime.UtcNow,

            Gallery = picList
        };

        var response = await _repositoryItems.Post(toPost);

       // var resp = _mapper.Map<ItemResponseForClientDTO>(response);

        return await GetByIdForUser(response.Id);
    }

    public async Task<bool> PostAdmin(ItemRequestDTO item)
    {
        var picList= new List<Picture>();
        var getLoggedUser = _userService.GetMe();

        foreach (var gallryId in item.GalleryIds)
        {
            picList.Add(await _repositoryPictures.GetById(gallryId));
        }

        var toPost = new Item
        {

            Name = item.Name,

            IsSold = false,

            OwnerUserId = getLoggedUser.Result.Id,
            
            Available = true,

            Desc = item.Desc,

            Price = item.Price,

            winningBidId = null,

            EndTime = item.EndTime,

            postedTime = DateTime.UtcNow,

            Gallery = picList
        };

        var temp = await _repositoryItems.Post(toPost);

        if (temp != null)
        {
            await _bidCloseServices.SetApproved(temp.Id);
            return true;
        }
        return false;
    }

    public async Task<bool> Update(ItemRequestForUpdateDTO item)
    {
        var itemMapped = _mapper.Map<Item>(item);

        var itemSearched = await _repositoryItems.GetById(item.Id);

        if (await _repositoryItems.Update(itemMapped) != null)
            return true;
        return false;
    }
    
    public async Task<Item> UpdateSold(int id)
    {
        var Sold = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
        if (Sold!= null)
        {
            Sold.IsSold= true;
            await _context.SaveChangesAsync();
            return Sold;
        }
        return null;
    }

    public async Task<int> GetNumberOfMyItems()
    {
        var count = await GetOwnItemsForUser();
        if (count != null)
        {
            return count.Count();
        }
        else
        {
            return 0;
        }
    }

    public async Task<List<ItemResponseDTO>> GetOwnItemsForUser()
    {
        var userId = await _userService.GetMe();
        var allitems = await _repositoryItemCustom.Get();
        var ownedItems = allitems.Where(i => i.OwnerUserId == userId.Id).ToList();
        var responseList = new List<ItemResponseDTO>();
        foreach(var item in ownedItems)
        {
            var itemResponse = new ItemResponseDTO
            {
                Id = item.Id,
                Name = item.Name,
                Desc = item.Desc,

                Price = item.Price,

                EndTime = item.EndTime.ToString(),

                postedTime = item.postedTime,

                Gallery = item.Gallery.Select(i => i.Id).ToList()
            };
            responseList.Add(itemResponse);
        }
        return responseList;      
    }

    public async Task<List<ItemOwnItemDTO>> GetOwnItemsByPage(int nr)
    {
        var userId = await _userService.GetMe();
        var allitems = await _repositoryItemCustom.Get();
        var ownedItems = allitems.Where(i => i.OwnerUserId == userId.Id).ToList();

        var maxPage = ownedItems.ToList().Count / 5;

        if (ownedItems.ToList().Count % 5 > 0)
        {
            maxPage++;
        }
        if (nr <= maxPage)
        {
            var result = ownedItems.ToList().GetRange(5 * nr - 5, 5 - ((nr * 5) - ownedItems.ToList().Count));

            var mappedItems = _mapper.Map<List<ItemOwnItemDTO>>(result);
            return mappedItems;
        }
        else
        {
            return null;
        }

    }

    public async Task<Item> Disable(int id)
    {
        await _bidCloseServices.SendEmailToUser(id); 
        return await _repositoryItemCustom.Disable(id);
    }

}